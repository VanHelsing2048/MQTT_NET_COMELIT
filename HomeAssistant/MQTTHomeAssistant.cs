using MQTT_NET_COMELIT.Comelit;
using MQTTnet;
using System.Text;
using static MQTT_NET_COMELIT.Utility.Utility;

namespace MQTT_NET_COMELIT.HomeAssistant
{
    public class MQTTHomeAssistant
    {
        public bool ConnectedAndLoggedIn { get; private set; }
        public MQTTComelit MQTTComelit { get; set; }

        readonly string MQTTUsername;
        readonly string MQTTPassword;
        readonly string MQTTIPAddress;

        IMqttClient MQTTClient;
        Task MQTTTask;
        private Dictionary<string, Device> _deviceCache = new();

        private const int RetryDelayMs = 5000;
        private int _retryCount = 0;

        public MQTTHomeAssistant(string username, string password, string ip, MQTTComelit mQTTComelit)
        {
            MQTTUsername = username;
            MQTTPassword = password;
            MQTTIPAddress = ip;
            MQTTComelit = mQTTComelit;
        }

        public void Start()
        {
            if (MQTTTask != null)
            {
                return;
            }

            MQTTTask = StartMQTT();
            _ = MQTTTask.ContinueWith(task => WriteLog($"HomeAssistant MQTT task failed: {task.Exception?.GetBaseException().Message}"), TaskContinuationOptions.OnlyOnFaulted);
        }

        private async Task StartMQTT()
        {
            _retryCount = 0;
            await ConnectWithRetry();
        }

        private async Task ConnectWithRetry()
        {
            try
            {
                using (MQTTClient = new MqttClientFactory().CreateMqttClient())
                {
                    var mqttClientOptions = new MqttClientOptionsBuilder()
                        .WithTcpServer(MQTTIPAddress)
                        .WithCredentials(MQTTUsername, MQTTPassword)
                        .WithCleanSession(true)
                        .Build();

                    var timeout = new CancellationTokenSource(5000);
                    MQTTClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedAsync;
                    MQTTClient.ConnectedAsync += MqttClient_ConnectedAsync;
                    MQTTClient.DisconnectedAsync += MqttClient_DisconnectedAsync;

                    var response = await MQTTClient.ConnectAsync(mqttClientOptions, timeout.Token);

                    if (response.ResultCode == MqttClientConnectResultCode.Success)
                    {
                        _retryCount = 0; // Reset retry count on successful connection
                        WriteLog("HomeAssistant MQTT client connected successfully.");
                        // Keep the connection alive
                        while (MQTTClient.IsConnected)
                        {
                            await Task.Delay(1000);
                        }
                    }
                    else
                    {
                        WriteLog($"HomeAssistant MQTT connection failed: {response.ResultCode}");
                        await HandleConnectionFailure();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                WriteLog("HomeAssistant MQTT connection timeout.");
                await HandleConnectionFailure();
            }
            catch (Exception ex)
            {
                WriteLog($"HomeAssistant MQTT error: {ex.Message}");
                await HandleConnectionFailure();
            }
            finally
            {
                ConnectedAndLoggedIn = false;
                WriteLog("The HomeAssistant MQTT client is disconnected.");
            }
        }

        private async Task HandleConnectionFailure()
        {
            _retryCount++;
            WriteLog($"Retrying HomeAssistant MQTT connection (attempt {_retryCount}) in {RetryDelayMs}ms...");
            await Task.Delay(RetryDelayMs);
            await ConnectWithRetry();
        }

        private Task MqttClient_DisconnectedAsync(MqttClientDisconnectedEventArgs args)
        {
            ConnectedAndLoggedIn = false;
            WriteLog("HomeAssistant MQTT client disconnected.");

            // Attempt to reconnect
            _ = Task.Run(async () => await HandleConnectionFailure());

            return Task.CompletedTask;
        }

        private Task MqttClient_ConnectedAsync(MqttClientConnectedEventArgs args)
        {
            WriteLog("The HomeAssistant MQTT client is connected.");

            ConnectedAndLoggedIn = true;
            RefreshDeviceSubscriptions();
            return Task.CompletedTask;
        }

        public void RefreshDeviceSubscriptions()
        {
            if (!ConnectedAndLoggedIn || MQTTComelit?.HomeStructure?.Areas == null)
            {
                WriteLog("HomeAssistant MQTT device subscriptions postponed: Comelit structure is not ready", LogLevel.Debug);
                return;
            }

            _deviceCache.Clear();
            foreach (Area area in MQTTComelit.HomeStructure.Areas)
            {
                foreach (Device device in area.Devices)
                {
                    if (device != null)
                    {
                        _deviceCache.TryAdd(device.GetIDForTopic(), device);
                    }
                }
            }

            // Subscribe to device-specific topics
            SubscribeToDeviceTopics();
        }

        private void SubscribeToDeviceTopics()
        {
            foreach (Area area in MQTTComelit.HomeStructure.Areas)
            {
                foreach (Device device in area.Devices)
                {
                    if (device?.CanSubscribe == true)
                    {
                        // Subscribe to main command topic
                        if (!string.IsNullOrWhiteSpace(device.CommandTopic))
                        {
                            SubscribeToTopic(device.CommandTopic);
                        }

                        // Subscribe to device-specific /set topics (NOT /state topics)
                        switch (device.SubType)
                        {
                            case Enums.OBJECT_SUBTYPE.DIMMER_LIGHT:
                                SubscribeToTopic($"home/lights/{device.GetIDForTopic()}/brightness/set");
                                break;

                            case Enums.OBJECT_SUBTYPE.ELECTRIC_BLIND:
                            case Enums.OBJECT_SUBTYPE.ENHANCED_ELECTRIC_BLIND:
                                // Subscribe only to position SET, not to state
                                SubscribeToTopic($"home/cover/{device.GetIDForTopic()}/position/set");
                                break;

                            case Enums.OBJECT_SUBTYPE.CLIMA_THERMOSTAT_DEHUMIDIFIER:
                                // Subscribe only to SET topics for temperature and humidity
                                SubscribeToTopic($"home/climate/{device.GetIDForTopic()}/target-temperature/set");
                                SubscribeToTopic($"home/climate/{device.GetIDForTopic()}/target-humidity/set");
                                break;
                        }
                    }
                }
            }
        }

        private void SubscribeToTopic(string topic)
        {
            try
            {
                MQTTClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build());
                WriteLog($"Subscribed to: {topic}", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                WriteLog($"Failed to subscribe to {topic}: {ex.Message}", LogLevel.Error);
            }
        }

        private Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
        {
            WriteLog("HomeAssistant NewMessage: " + args.ApplicationMessage.Topic + " - " + Encoding.Default.GetString(args.ApplicationMessage.Payload), LogLevel.Debug);
            Device device = GetDeviceFromTopic(args.ApplicationMessage.Topic);
            string payload = Encoding.Default.GetString(args.ApplicationMessage.Payload);

            // Route based on topic pattern
            RouteCommandByTopic(args.ApplicationMessage.Topic, device, payload);
            return Task.CompletedTask;
        }

        private void RouteCommandByTopic(string topic, Device device, string payload)
        {
            if (topic.Contains("/brightness/set"))
            {
                MQTTComelit.UpdateDeviceBrightness(device, payload);
            }
            else if (topic.Contains("/target-temperature/set"))
            {
                MQTTComelit.UpdateDeviceTemperature(device, payload);
            }
            else if (topic.Contains("/target-humidity/set"))
            {
                MQTTComelit.UpdateDeviceHumidity(device, payload);
            }
            else if (topic.Contains("/position"))
            {
                MQTTComelit.UpdateDevicePosition(device, payload);
            }
            else
            {
                // Standard on/off command for all other topics
                MQTTComelit.UpdateDevice(device, payload);
            }
        }

        private Device GetDeviceFromTopic(string topic)
        {
            // First, try to find in cache
            foreach (var cached in _deviceCache.Where(x => topic.Contains(x.Key)))
            {
                return cached.Value;
            }

            // If not in cache, search and populate cache
            foreach (Area area in MQTTComelit.HomeStructure.Areas)
            {
                foreach (Device dev in area.Devices)
                {
                    if (dev != null && topic.Contains(dev.GetIDForTopic()))
                    {
                        // Cache the device for future lookups
                        _deviceCache.TryAdd(dev.GetIDForTopic(), dev);
                        return dev;
                    }
                }
            }

            WriteLog($"Device not found for topic: {topic}", LogLevel.Warning);
            return null;
        }

        public void Publish(string topic, string payload, bool retain = false)
        {
            if (!ConnectedAndLoggedIn || MQTTClient?.IsConnected != true)
            {
                WriteLog($"HomeAssistant publish skipped because MQTT is not connected: {topic}", LogLevel.Warning);
                return;
            }

            if (ShouldNormalizeBinaryPayload(topic))
            {
                if (payload == "1") payload = "ON";
                if (payload == "0") payload = "OFF";
            }
            WriteLog("HomeAssistant Publish: " + topic + " - " + payload, LogLevel.Debug);
            MQTTClient.PublishAsync(new MqttApplicationMessage() { Topic = topic, PayloadSegment = Encoding.ASCII.GetBytes(payload), Retain = retain });
        }

        private static bool ShouldNormalizeBinaryPayload(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                return false;
            }

            return !topic.Contains("/climate/")
                && !topic.Contains("/sensor/")
                && !topic.Contains("/brightness/")
                && !topic.Contains("/position/");
        }
    }
}
