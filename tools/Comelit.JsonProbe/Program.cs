using System.Text;
using MQTTnet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

var options = ProbeOptions.Parse(args);
if (options.ShowHelp)
{
    ProbeOptions.PrintHelp();
    return options.HasError ? 1 : 0;
}

Directory.CreateDirectory(options.OutputDirectory);

await using var probe = new ComelitProbe(options);
await probe.ConnectAsync();

if (options.HasClimateCommand)
{
    await probe.ExecuteClimateActionAsync(CancellationToken.None);
}
else if (options.Watch)
{
    Console.WriteLine($"Watching {options.DeviceId}. Press Ctrl+C to stop.");
    using var stop = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) =>
    {
        e.Cancel = true;
        stop.Cancel();
    };

    string? lastSnapshot = null;
    while (!stop.IsCancellationRequested)
    {
        string? current = await probe.PrintDeviceAsync(save: true, previousSnapshot: lastSnapshot, stop.Token);
        if (current != null)
        {
            lastSnapshot = current;
        }

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(options.IntervalSeconds), stop.Token);
        }
        catch (OperationCanceledException)
        {
            break;
        }
    }
}
else
{
    await probe.PrintDeviceAsync(options.Save, previousSnapshot: null, CancellationToken.None);
}

return 0;

internal sealed class ComelitProbe : IAsyncDisposable
{
    private readonly ProbeOptions _options;
    private readonly IMqttClient _client;
    private readonly string _clientId;
    private readonly string _rxTopic;
    private readonly string _txTopic;
    private readonly object _gate = new();
    private readonly Dictionary<int, TaskCompletionSource<JObject>> _pendingResponses = new();
    private int _sequenceId = 1;
    private int _agentId;
    private string _sessionToken = string.Empty;

    public ComelitProbe(ProbeOptions options)
    {
        _options = options;
        _clientId = string.IsNullOrWhiteSpace(options.ClientId)
            ? $"json_probe_{Guid.NewGuid():N}"
            : options.ClientId;
        _rxTopic = $"HSrv/{options.Serial}/rx/{_clientId}";
        _txTopic = $"HSrv/{options.Serial}/tx/{_clientId}";
        _client = new MqttClientFactory().CreateMqttClient();
        _client.ApplicationMessageReceivedAsync += OnMessageAsync;
    }

    public async Task ConnectAsync()
    {
        var mqttOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(_options.Host, _options.Port)
            .WithCredentials(_options.MqttUser, _options.MqttPassword)
            .WithClientId(_clientId)
            .Build();

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(_options.TimeoutSeconds));
        await _client.ConnectAsync(mqttOptions, timeout.Token);
        await _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(_txTopic).Build(), timeout.Token);

        Console.WriteLine($"Connected. tx={_txTopic} rx={_rxTopic}");
        await LoginAsync(timeout.Token);
    }

    public async ValueTask DisposeAsync()
    {
        if (_client.IsConnected)
        {
            await _client.DisconnectAsync();
        }

        _client.Dispose();
    }

    public async Task<string?> PrintDeviceAsync(bool save, string? previousSnapshot, CancellationToken cancellationToken)
    {
        JObject status = await RequestStatusAsync(cancellationToken);
        JObject? device = FindDevice(status, _options.DeviceId);

        if (device == null)
        {
            Console.WriteLine($"Device '{_options.DeviceId}' not found in status payload.");
            return null;
        }

        string json = device.ToString(Formatting.Indented);
        if (previousSnapshot == json)
        {
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - no change");
            return json;
        }

        Console.WriteLine();
        Console.WriteLine($"--- {_options.DeviceId} @ {DateTime.Now:yyyy-MM-dd HH:mm:ss} ---");
        Console.WriteLine(json);

        if (save || _options.Watch)
        {
            string fileName = $"{SanitizeFileName(_options.DeviceId)}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            string path = Path.Combine(_options.OutputDirectory, fileName);
            await File.WriteAllTextAsync(path, json + Environment.NewLine, cancellationToken);
            Console.WriteLine($"Saved: {path}");
        }

        return json;
    }

    public async Task ExecuteClimateActionAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine();
        Console.WriteLine("=== Before ===");
        await PrintDeviceAsync(save: _options.Save, previousSnapshot: null, cancellationToken);

        ClimateActionPayload action = _options.GetClimateActionPayload();
        string actionName = _options.GetClimateActionName();
        int seq = NextSequence();
        var payload = new JObject
        {
            ["req_type"] = 1,
            ["req_sub_type"] = 3,
            ["seq_id"] = seq,
            ["act_type"] = action.ActType,
            ["sessiontoken"] = _sessionToken,
            ["obj_id"] = _options.DeviceId,
            ["act_params"] = new JArray(action.Parameters.Select(parameter => new JValue(parameter)))
        };

        Console.WriteLine();
        Console.WriteLine($"=== Sending {actionName} to {_options.DeviceId} ===");
        Console.WriteLine($"act_type={action.ActType}, act_params=[{string.Join(", ", action.Parameters)}]");
        JObject reply = await PublishAndWaitStatusStepAsync(payload, seq, cancellationToken);
        Console.WriteLine($"Reply: {reply.ToString(Formatting.None)}");

        if (_options.AfterDelaySeconds > 0)
        {
            await Task.Delay(TimeSpan.FromSeconds(_options.AfterDelaySeconds), cancellationToken);
        }

        Console.WriteLine();
        Console.WriteLine("=== After ===");
        await PrintDeviceAsync(save: _options.Save, previousSnapshot: null, cancellationToken);
    }

    private async Task LoginAsync(CancellationToken cancellationToken)
    {
        JObject announce = await PublishAndWaitStatusStepAsync(
            new JObject
            {
                ["req_type"] = 13,
                ["req_sub_type"] = -1,
                ["seq_id"] = NextSequence(),
                ["agent_type"] = 0
            },
            expectedSeq: 1,
            cancellationToken);

        _agentId = announce["out_data"]?.First?["agent_id"]?.Value<int>() ?? 0;
        if (_agentId == 0)
        {
            throw new InvalidOperationException("Comelit announce did not return an agent_id.");
        }

        JObject login = await PublishAndWaitStatusStepAsync(
            new JObject
            {
                ["req_type"] = 5,
                ["req_sub_type"] = -1,
                ["seq_id"] = NextSequence(),
                ["agent_type"] = 0,
                ["agent_id"] = _agentId,
                ["user_name"] = _options.HubUser,
                ["password"] = _options.HubPassword
            },
            expectedSeq: 2,
            cancellationToken);

        _sessionToken = login["sessiontoken"]?.Value<string>() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(_sessionToken))
        {
            throw new InvalidOperationException("Comelit login did not return a sessiontoken.");
        }

        Console.WriteLine($"Logged in. agent_id={_agentId}");
    }

    private Task<JObject> RequestStatusAsync(CancellationToken cancellationToken)
    {
        int seq = NextSequence();
        return PublishAndWaitStatusStepAsync(
            new JObject
            {
                ["req_type"] = 0,
                ["req_sub_type"] = -1,
                ["seq_id"] = seq,
                ["obj_id"] = _options.RootElement,
                ["detail_level"] = 1,
                ["agent_id"] = _agentId,
                ["sessiontoken"] = _sessionToken
            },
            expectedSeq: seq,
            cancellationToken);
    }

    private async Task<JObject> PublishAndWaitStatusStepAsync(JObject payload, int expectedSeq, CancellationToken cancellationToken)
    {
        var completion = new TaskCompletionSource<JObject>(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (_gate)
        {
            _pendingResponses[expectedSeq] = completion;
        }

        try
        {
            await PublishAsync(payload, cancellationToken);
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));
            using (timeout.Token.Register(() => completion.TrySetCanceled(timeout.Token)))
            {
                return await completion.Task;
            }
        }
        finally
        {
            lock (_gate)
            {
                _pendingResponses.Remove(expectedSeq);
            }
        }
    }

    private async Task PublishAsync(JObject payload, CancellationToken cancellationToken)
    {
        string json = payload.ToString(Formatting.None);
        if (_options.Verbose)
        {
            Console.WriteLine($"> {_rxTopic} {json}");
        }

        var message = new MqttApplicationMessage
        {
            Topic = _rxTopic,
            PayloadSegment = Encoding.UTF8.GetBytes(json)
        };
        await _client.PublishAsync(message, cancellationToken);
    }

    private Task OnMessageAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        string text = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);
        if (_options.Verbose)
        {
            Console.WriteLine($"< {args.ApplicationMessage.Topic} {text}");
        }

        JObject? json;
        try
        {
            json = JObject.Parse(text);
        }
        catch
        {
            return Task.CompletedTask;
        }

        lock (_gate)
        {
            int seq = json["seq_id"]?.Value<int>() ?? -1;
            if (_pendingResponses.TryGetValue(seq, out TaskCompletionSource<JObject>? completion))
            {
                completion.TrySetResult(json);
            }
        }

        return Task.CompletedTask;
    }

    private int NextSequence() => _sequenceId++;

    private static JObject? FindDevice(JToken token, string deviceId)
    {
        var matches = new List<JObject>();
        CollectMatches(token, deviceId, matches);

        return matches
            .OrderByDescending(ScoreDeviceMatch)
            .FirstOrDefault();
    }

    private static void CollectMatches(JToken token, string deviceId, List<JObject> matches)
    {
        if (token is JObject obj)
        {
            if (obj["data"] is JObject data && IsMatchingObject(data, deviceId))
            {
                matches.Add(data);
            }
            else if (IsMatchingObject(obj, deviceId))
            {
                matches.Add(obj);
            }

            foreach (JProperty property in obj.Properties())
            {
                CollectMatches(property.Value, deviceId, matches);
            }
        }
        else if (token is JArray array)
        {
            foreach (JToken item in array)
            {
                CollectMatches(item, deviceId, matches);
            }
        }
    }

    private static bool IsMatchingObject(JObject obj, string deviceId)
    {
        return string.Equals(obj["id"]?.Value<string>(), deviceId, StringComparison.OrdinalIgnoreCase)
            || string.Equals(obj["objectId"]?.Value<string>(), deviceId, StringComparison.OrdinalIgnoreCase);
    }

    private static int ScoreDeviceMatch(JObject obj)
    {
        int score = 0;
        if (obj["type"] != null) score += 10;
        if (obj["sub_type"] != null) score += 10;
        if (obj["descrizione"] != null) score += 3;
        if (obj["temperatura"] != null) score += 3;
        if (obj["umidita"] != null) score += 3;
        return score;
    }

    private static string SanitizeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars().ToHashSet();
        var builder = new StringBuilder(value.Length);
        foreach (char ch in value)
        {
            builder.Append(invalid.Contains(ch) ? '_' : ch);
        }

        return builder.ToString().Replace('#', '_').Replace('.', '_');
    }
}

internal sealed record ProbeOptions
{
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 1883;
    public string Serial { get; init; } = string.Empty;
    public string DeviceId { get; init; } = string.Empty;
    public string RootElement { get; init; } = "GEN#17#13#1";
    public string MqttUser { get; init; } = "hsrv-user";
    public string MqttPassword { get; init; } = "sf1nE9bjPc";
    public string HubUser { get; init; } = "admin";
    public string HubPassword { get; init; } = "admin";
    public string? ClientId { get; init; }
    public string? ClimateAction { get; init; }
    public double? TargetTemperature { get; init; }
    public int? TargetHumidity { get; init; }
    public string OutputDirectory { get; init; } = "snapshots";
    public int IntervalSeconds { get; init; } = 5;
    public int AfterDelaySeconds { get; init; } = 2;
    public int TimeoutSeconds { get; init; } = 10;
    public bool Save { get; init; }
    public bool Watch { get; init; }
    public bool Verbose { get; init; }
    public bool ShowHelp { get; init; }
    public bool HasError { get; init; }

    public static ProbeOptions Parse(string[] args)
    {
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        bool save = false;
        bool watch = false;
        bool verbose = false;
        bool help = false;
        bool error = false;

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            if (arg is "--help" or "-h" or "/?")
            {
                help = true;
                continue;
            }

            if (arg is "--save")
            {
                save = true;
                continue;
            }

            if (arg is "--watch")
            {
                watch = true;
                continue;
            }

            if (arg is "--verbose" or "-v")
            {
                verbose = true;
                continue;
            }

            if (!arg.StartsWith("--", StringComparison.Ordinal))
            {
                Console.Error.WriteLine($"Unexpected argument: {arg}");
                error = true;
                continue;
            }

            string key = arg[2..];
            if (i + 1 >= args.Length || args[i + 1].StartsWith("--", StringComparison.Ordinal))
            {
                Console.Error.WriteLine($"Missing value for --{key}");
                error = true;
                continue;
            }

            values[key] = args[++i];
        }

        string host = Get(values, "host");
        string serial = Get(values, "serial", "mac");
        string climateAction = Get(values, "climate-action", "action");
        double? targetTemperature = GetDouble(values, "temperature", "target-temperature", "temp");
        int? targetHumidity = GetIntNullable(values, "humidity", "target-humidity", "hum");
        string device = Get(values, "device", "device-id", "id");
        if ((!string.IsNullOrWhiteSpace(climateAction) || targetTemperature.HasValue || targetHumidity.HasValue)
            && string.IsNullOrWhiteSpace(device))
        {
            device = "DOM#CL#17.1";
        }

        if (!help)
        {
            Require(host, "--host", ref error);
            Require(serial, "--serial", ref error);
            Require(device, "--device", ref error);
            if (!string.IsNullOrWhiteSpace(climateAction) && !ClimateActionPayload.IsKnown(climateAction))
            {
                Console.Error.WriteLine($"Unknown --climate-action '{climateAction}'.");
                error = true;
            }

            int climateCommandCount = 0;
            if (!string.IsNullOrWhiteSpace(climateAction))
            {
                climateCommandCount++;
            }

            if (targetTemperature.HasValue)
            {
                climateCommandCount++;
                if (targetTemperature < 15 || targetTemperature > 30)
                {
                    Console.Error.WriteLine("--temperature must be between 15 and 30 degrees.");
                    error = true;
                }
            }

            if (targetHumidity.HasValue)
            {
                climateCommandCount++;
                if (targetHumidity < 30 || targetHumidity > 80)
                {
                    Console.Error.WriteLine("--humidity must be between 30 and 80 percent.");
                    error = true;
                }
            }

            if (climateCommandCount > 1)
            {
                Console.Error.WriteLine("Use only one climate command at a time: --climate-action, --temperature, or --humidity.");
                error = true;
            }
        }

        return new ProbeOptions
        {
            Host = host,
            Port = GetInt(values, "port", 1883),
            Serial = serial,
            DeviceId = device,
            RootElement = Get(values, "root", "root-element") is { Length: > 0 } root ? root : "GEN#17#13#1",
            MqttUser = Get(values, "mqtt-user") is { Length: > 0 } mqttUser ? mqttUser : "hsrv-user",
            MqttPassword = Get(values, "mqtt-password") is { Length: > 0 } mqttPassword ? mqttPassword : "sf1nE9bjPc",
            HubUser = Get(values, "hub-user", "user") is { Length: > 0 } hubUser ? hubUser : "admin",
            HubPassword = Get(values, "hub-password", "password") is { Length: > 0 } hubPassword ? hubPassword : "admin",
            ClientId = Get(values, "client-id") is { Length: > 0 } clientId ? clientId : null,
            ClimateAction = string.IsNullOrWhiteSpace(climateAction) ? null : climateAction,
            TargetTemperature = targetTemperature,
            TargetHumidity = targetHumidity,
            OutputDirectory = Get(values, "output", "out") is { Length: > 0 } output ? output : "snapshots",
            IntervalSeconds = GetInt(values, "interval", 5),
            AfterDelaySeconds = GetInt(values, "after-delay", 2),
            TimeoutSeconds = GetInt(values, "timeout", 10),
            Save = save,
            Watch = watch,
            Verbose = verbose,
            ShowHelp = help || error,
            HasError = error
        };
    }

    public static void PrintHelp()
    {
        Console.WriteLine("""
        Comelit.JsonProbe - dumps one Comelit device JSON from the hub MQTT status payload.

        Required:
          --host <ip>              Comelit hub MQTT host
          --serial <serial/mac>    Hub serial used in HSrv/<serial>/...
          --device <id>            Device id, for example DOM#CL#18.1

        Optional:
          --port <port>            Default: 1883
          --mqtt-user <user>       Default: hsrv-user
          --mqtt-password <pwd>    Default: sf1nE9bjPc
          --hub-user <user>        Default: admin
          --hub-password <pwd>     Default: admin
          --root <id>              Default: GEN#17#13#1
          --save                   Save one snapshot under snapshots/
          --watch                  Poll repeatedly and save snapshots on changes
          --interval <seconds>     Watch interval. Default: 5
          --output <folder>        Snapshot folder. Default: snapshots
          --verbose                Print raw MQTT payloads

        Climate action mode:
          --climate-action <name>  Send one climate action, then print before/after JSON.
                                   Defaults --device to DOM#CL#17.1 when omitted.
          --temperature <degrees>  Send target temperature, 15-30 degrees.
          --humidity <percent>     Send target humidity, 30-80 percent.
          --after-delay <seconds>  Delay before after snapshot. Default: 2

        Climate actions:
          thermo-on, thermo-off, umi-on, umi-off, both-on, both-off,
          thermo-auto, thermo-manual, umi-auto, umi-manual,
          heat, cool

        Example:
          dotnet run --project tools/Comelit.JsonProbe -- --host 192.168.1.51 --serial 00252917071D --device "DOM#CL#18.1" --save

          dotnet run --project tools/Comelit.JsonProbe -- --host 192.168.1.51 --serial 00252917071D --climate-action umi-on --save --verbose
        """);
    }

    public bool HasClimateCommand =>
        !string.IsNullOrWhiteSpace(ClimateAction)
        || TargetTemperature.HasValue
        || TargetHumidity.HasValue;

    public string GetClimateActionName()
    {
        if (!string.IsNullOrWhiteSpace(ClimateAction))
        {
            return ClimateAction;
        }

        if (TargetTemperature.HasValue)
        {
            return $"temperature {TargetTemperature.Value:F1}";
        }

        if (TargetHumidity.HasValue)
        {
            return $"humidity {TargetHumidity.Value}";
        }

        throw new InvalidOperationException("No climate command was specified.");
    }

    public ClimateActionPayload GetClimateActionPayload()
    {
        if (!string.IsNullOrWhiteSpace(ClimateAction))
        {
            return ClimateActionPayload.FromName(ClimateAction);
        }

        if (TargetTemperature.HasValue)
        {
            return ClimateActionPayload.ForTemperature(TargetTemperature.Value);
        }

        if (TargetHumidity.HasValue)
        {
            return ClimateActionPayload.ForHumidity(TargetHumidity.Value);
        }

        throw new InvalidOperationException("No climate command was specified.");
    }

    private static string Get(Dictionary<string, string?> values, params string[] keys)
    {
        foreach (string key in keys)
        {
            if (values.TryGetValue(key, out string? value))
            {
                return value ?? string.Empty;
            }
        }

        return string.Empty;
    }

    private static int GetInt(Dictionary<string, string?> values, string key, int fallback)
    {
        return values.TryGetValue(key, out string? value)
            && int.TryParse(value, out int parsed)
            && parsed > 0
            ? parsed
            : fallback;
    }

    private static int? GetIntNullable(Dictionary<string, string?> values, params string[] keys)
    {
        string value = Get(values, keys);
        return int.TryParse(value, out int parsed) ? parsed : null;
    }

    private static double? GetDouble(Dictionary<string, string?> values, params string[] keys)
    {
        string value = Get(values, keys);
        return double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double parsed)
            ? parsed
            : null;
    }

    private static void Require(string value, string optionName, ref bool error)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        Console.Error.WriteLine($"Missing required option {optionName}");
        error = true;
    }
}

internal sealed record ClimateActionPayload(int ActType, int[] Parameters)
{
    private static readonly IReadOnlyDictionary<string, ClimateActionPayload> Actions =
        new Dictionary<string, ClimateActionPayload>(StringComparer.OrdinalIgnoreCase)
        {
            ["thermo-off"] = new(0, [0]),
            ["thermo-on"] = new(0, [1]),
            ["umi-off"] = new(0, [2]),
            ["umi-on"] = new(0, [3]),
            ["both-off"] = new(0, [4]),
            ["both-on"] = new(0, [5]),
            ["thermo-auto"] = new(13, [1]),
            ["thermo-manual"] = new(13, [2]),
            ["umi-auto"] = new(23, [1]),
            ["umi-manual"] = new(23, [2]),
            ["cool"] = new(4, [0]),
            ["heat"] = new(4, [1])
        };

    public static bool IsKnown(string name) => Actions.ContainsKey(name);

    public static ClimateActionPayload ForTemperature(double temperature)
    {
        int value = (int)Math.Round(temperature * 10, MidpointRounding.AwayFromZero);
        return new ClimateActionPayload(2, [value]);
    }

    public static ClimateActionPayload ForHumidity(int humidity) => new(19, [humidity]);

    public static ClimateActionPayload FromName(string name)
    {
        if (Actions.TryGetValue(name, out ClimateActionPayload? action))
        {
            return action;
        }

        throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown climate action.");
    }
}
