using MQTT_NET_COMELIT.HomeAssistant;
using MQTTnet;
using Newtonsoft.Json;
using System.Text;
using static MQTT_NET_COMELIT.Comelit.ComelitMessages;
using static MQTT_NET_COMELIT.Comelit.JsonParsing;
using static MQTT_NET_COMELIT.Utility.Utility;

namespace MQTT_NET_COMELIT.Comelit
{
    public partial class MQTTComelit
    {
        public MQTTHomeAssistant MQTTHomeAssistant { get; set; }
        private string LastCommand = string.Empty;
        private DeviceStateManager _deviceStateManager = new();


        public MQTTComelit(string username, string password, string mac, string ip, string root, int pollingTime)
        {
            MQTTUsername = username;
            MQTTPassword = password;
            HubMACAddress = mac;
            ServerIPAddress = ip;
            ComelitROOTElement = root;

            _pollingMs = pollingTime * 1000;

            SubscribeTopic = $"HSrv/{HubMACAddress}/tx/{MQTTClientID}";
            PublishTopic = $"HSrv/{HubMACAddress}/rx/{MQTTClientID}";
        }

        public void Start()
        {
            if (MQTTTask != null)
            {
                return;
            }

            WriteLog("Starting Comelit MQTT");
            MQTTTask = StartMQTT();
            _ = MQTTTask.ContinueWith(task => WriteLog($"Comelit MQTT task failed: {task.Exception?.GetBaseException().Message}"), TaskContinuationOptions.OnlyOnFaulted);
        }

        private void MessageManager(string payload)
        {
            if (!MQTTLoggedIn) { LoginSequence(payload); } else { ManageNewMessage(payload); }
        }

        private void ManageNewMessage(string message)
        {
            Header GetStep = JsonConvert.DeserializeObject<Header>(message);
            if (GetStep != null)
            {
                WriteLog($"COMELIT NEW MESSAGE: seq={GetStep.SeqID}, obj={GetStep.ObjID}, message={GetStep.Message}", LogLevel.Debug);
                if (GetStep.ObjID != null && ConnectedAndLoggedIn && GetStep.Message == null)
                {
                    OutData recDev = GetStep.OutData.First();

                    foreach (Area area in HomeStructure.Areas)
                    {
                        Device device = area.Devices.FirstOrDefault(d => d?.ID == recDev.ID);
                        if (device != null)
                        {
                            // Update device state from Comelit message
                            device.Status = recDev.Status;

                            // Copy device-specific properties from recDev if needed
                            if (device is DimmerLight dimmer && !string.IsNullOrEmpty(recDev.Bright))
                            {
                                dimmer.Bright = recDev.Bright;
                            }
                            if (device is ElectricBlind blind && !string.IsNullOrEmpty(recDev.OpenStatus))
                            {
                                blind.OpenStatus = recDev.OpenStatus;
                            }
                            if (device is Clima clima)
                            {
                                if (!string.IsNullOrEmpty(recDev.Temperatura))
                                {
                                    clima.Temperatura = NormalizeComelitTemperature(recDev.Temperatura);
                                }
                                if (!string.IsNullOrEmpty(recDev.Umidita))
                                    clima.Umidita = recDev.Umidita;
                                if (!string.IsNullOrEmpty(recDev.SogliaAttiva))
                                    clima.SogliaAttiva = recDev.SogliaAttiva;
                                if (!string.IsNullOrEmpty(recDev.SogliaAttivaUmi))
                                    clima.SogliaAttivaUmi = recDev.SogliaAttivaUmi;
                                if (!string.IsNullOrEmpty(recDev.EstInv))
                                    clima.EstInv = recDev.EstInv;
                                if (!string.IsNullOrEmpty(recDev.AutoMan))
                                    clima.AutoMan = recDev.AutoMan;
                                if (!string.IsNullOrEmpty(recDev.PowerSt))
                                    clima.PowerSt = recDev.PowerSt;
                                if (!string.IsNullOrEmpty(recDev.AutoManUmi))
                                    clima.AutoManUmi = recDev.AutoManUmi;
                                if (!string.IsNullOrEmpty(recDev.SemiautoEnabled))
                                    clima.SemiautoEnabled = recDev.SemiautoEnabled;
                            }
                            if (device is ComelitSensor sensor)
                            {
                                if (!string.IsNullOrEmpty(recDev.InstantPower))
                                    sensor.InstantPower = recDev.InstantPower;
                                if (!string.IsNullOrEmpty(recDev.LabelValue))
                                    sensor.LabelValue = recDev.LabelValue;
                            }

                            // Check if state has actually changed before publishing
                            if (!_deviceStateManager.HasChanged(device))
                            {
                                WriteLog($"[SKIP-PUBLISH] Device {device.ID} ({device.SubType}): No state change detected, skipping MQTT publish to HA", LogLevel.Debug);
                                break; // Skip publishing for this device
                            }

                            // Publish only if state changed
                            switch (device.SubType)
                            {
                                case Enums.OBJECT_SUBTYPE.DIGITAL_LIGHT:
                                    MQTTHomeAssistant.Publish(device.StatusTopic, device.StatusONOFF);
                                    break;
                                case Enums.OBJECT_SUBTYPE.DIMMER_LIGHT:
                                    MQTTHomeAssistant.Publish(device.StatusTopic, device.StatusONOFF);
                                    if (device is DimmerLight dimmer1 && !string.IsNullOrEmpty(dimmer1.Bright))
                                    {
                                        MQTTHomeAssistant.Publish($"home/lights/{device.GetIDForTopic()}/brightness/state", dimmer1.Bright);
                                    }
                                    break;
                                case Enums.OBJECT_SUBTYPE.IRRIGATION_VALVE:
                                    MQTTHomeAssistant.Publish(device.StatusTopic, device.StatusONOFF);
                                    break;
                                case Enums.OBJECT_SUBTYPE.OTHER_DIGIT:
                                    MQTTHomeAssistant.Publish(device.StatusTopic, device.StatusONOFF);
                                    break;
                                case Enums.OBJECT_SUBTYPE.ELECTRIC_BLIND:
                                case Enums.OBJECT_SUBTYPE.ENHANCED_ELECTRIC_BLIND:
                                    MQTTHomeAssistant.Publish(device.StatusTopic, device.CoverState);
                                    if (device is ElectricBlind blind1 && !string.IsNullOrEmpty(blind1.OpenStatus))
                                    {
                                        MQTTHomeAssistant.Publish($"home/cover/{device.GetIDForTopic()}/position/state", blind1.OpenStatus);
                                    }
                                    break;
                                case Enums.OBJECT_SUBTYPE.CLIMA_THERMOSTAT_DEHUMIDIFIER:
                                    if (device is Clima clima1)
                                    {
                                        if (!string.IsNullOrEmpty(clima1.Temperatura))
                                        {
                                            MQTTHomeAssistant.Publish($"home/climate/{device.GetIDForTopic()}/current-temperature/state", NormalizeComelitTemperature(clima1.Temperatura));
                                        }
                                        PublishClimateDerivedState(clima1);
                                        if (!string.IsNullOrEmpty(clima1.Umidita))
                                        {
                                            MQTTHomeAssistant.Publish($"home/climate/{device.GetIDForTopic()}/current-humidity/state", clima1.Umidita);
                                        }
                                        MQTTHomeAssistant.Publish(device.StatusTopic, device.StatusONOFF);
                                    }
                                    break;
                            }
                            if (device is ComelitSensor sensor1)
                            {
                                MQTTHomeAssistant.Publish(sensor1.StatusTopic, sensor1.SensorValue);
                            }
                            switch (device.Type)
                            {
                                case Enums.OBJECT_TYPE.INPUT:
                                    MQTTHomeAssistant.Publish(device?.StatusTopic, device.StatusONOFF);
                                    break;
                                case Enums.OBJECT_TYPE.RULE:
                                    MQTTHomeAssistant.Publish(device?.StatusTopic, device.StatusONOFF);
                                    break;
                            }
                            break;
                        }
                    }
                }
                else if (GetStep.Message != null)
                {
                    switch (GetStep.Message)
                    {
                        case "invalid token":
                            LoginSequence();
                            // Rilanciare la sottoscrizione a tutti gli elementi con il nuovo sessionToken
                            SubscribeToDeviceValueChange();
                            // Inviare nuovamente il comando
                            WriteLog("Sending LastCommand after LoginSequence: " + LastCommand, LogLevel.Debug);
                            MQTTClient.PublishAsync(new MqttApplicationMessage() { Topic = PublishTopic, PayloadSegment = Encoding.ASCII.GetBytes(LastCommand) });
                            break;
                    }
                }
                else if (IsPolling)
                {
                    OutData data = GetStep.OutData[0];
                    if (data.SubType == PollingDevice.SubType && data.ID == PollingDevice.ID)
                    {
                        MQTTHomeAssistant.Publish(PollingStatus, "ON", true);
                    }
                }
            }
        }

        internal void UpdateDevice(Device device, string payload)
        {
            if (device == null)
            {
                WriteLog("MQTTComelit: UpdateDevice has NULL device", LogLevel.Warning);
            }
            else
            {
                LastCommand = string.Empty;
                switch (device.SubType)
                {
                    case Enums.OBJECT_SUBTYPE.DIGITAL_LIGHT:
                        if (payload == "ON") { device.Status = "1"; }
                        if (payload == "OFF") { device.Status = "0"; }
                        LastCommand = BuildOnOffCommand(SessionToken, device.ID, device.Status);
                        break;
                    case Enums.OBJECT_SUBTYPE.DIMMER_LIGHT:
                        if (payload == "ON") { device.Status = "1"; }
                        if (payload == "OFF") { device.Status = "0"; }
                        if (int.TryParse(payload, out int brightness) && brightness >= 0 && brightness <= 255)
                        {
                            // Payload è un valore di brightness (0-255)
                            if (device is DimmerLight dimmer)
                            {
                                dimmer.Bright = brightness.ToString();
                                LastCommand = BuildDimmerCommand(SessionToken, device.ID, brightness);
                            }
                        }
                        else
                        {
                            LastCommand = BuildOnOffCommand(SessionToken, device.ID, device.Status);
                        }
                        break;
                    case Enums.OBJECT_SUBTYPE.ELECTRIC_BLIND:
                    case Enums.OBJECT_SUBTYPE.ENHANCED_ELECTRIC_BLIND:
                        if (payload == "OPEN")
                        {
                            device.Status = "1";
                            LastCommand = BuildBlindCommand(SessionToken, device.ID, "OPEN");
                        }
                        else if (payload == "CLOSE")
                        {
                            device.Status = "0";
                            LastCommand = BuildBlindCommand(SessionToken, device.ID, "CLOSE");
                        }
                        else if (payload == "STOP")
                        {
                            LastCommand = BuildBlindCommand(SessionToken, device.ID, "STOP");
                        }
                        else if (int.TryParse(payload, out int position) && position >= 0 && position <= 100)
                        {
                            // Payload è una posizione 0-100%
                            if (device is ElectricBlind blind)
                            {
                                blind.OpenStatus = position.ToString();
                                LastCommand = BuildBlindPositionCommand(SessionToken, device.ID, position);
                            }
                        }
                        break;
                    case Enums.OBJECT_SUBTYPE.CLIMA_THERMOSTAT_DEHUMIDIFIER:
                        // Temperature command handling
                        if (double.TryParse(payload, System.Globalization.CultureInfo.InvariantCulture, out double temperature) && temperature >= 15 && temperature <= 30)
                        {
                            if (device is Clima clima)
                            {
                                clima.Temperatura = temperature.ToString("F1");
                                LastCommand = BuildClimaTemperatureCommand(SessionToken, device.ID, temperature);
                            }
                        }
                        break;
                    case Enums.OBJECT_SUBTYPE.IRRIGATION_VALVE:
                        if (payload == "ON") { device.Status = "1"; }
                        if (payload == "OFF") { device.Status = "0"; }
                        LastCommand = BuildOnOffCommand(SessionToken, device.ID, device.Status);
                        break;
                    case Enums.OBJECT_SUBTYPE.OTHER_DIGIT:
                        if (payload == "ON") { device.Status = "1"; }
                        if (payload == "OFF") { device.Status = "0"; }
                        LastCommand = BuildOnOffCommand(SessionToken, device.ID, device.Status);
                        break;
                    case Enums.OBJECT_SUBTYPE.OTHER_TMP:
                    case Enums.OBJECT_SUBTYPE.AUTOMATION:
                        if (payload == "PRESS" || payload == "ON") { device.Status = "1"; }
                        if (payload == "OFF") { device.Status = "0"; }
                        LastCommand = BuildOnOffCommand(SessionToken, device.ID, device.Status);
                        break;
                }
                if (string.IsNullOrEmpty(LastCommand))
                {
                    WriteLog($"Comelit {device.SubType}: no command built for payload '{payload}'", LogLevel.Warning);
                    return;
                }
                WriteLog($"Comelit {device.SubType}: {LastCommand}", LogLevel.Debug);
                MQTTClient.PublishAsync(new MqttApplicationMessage() { Topic = PublishTopic, PayloadSegment = Encoding.ASCII.GetBytes(LastCommand) });

            }
        }

        internal void UpdateDeviceBrightness(Device device, string payload)
        {
            try
            {
                if (device == null)
                {
                    WriteLog("UpdateDeviceBrightness - Device is null", LogLevel.Error);
                    return;
                }

                if (!(device is DimmerLight dimmer))
                {
                    WriteLog($"UpdateDeviceBrightness - Device '{device.ID}' is not a DimmerLight (Type: {device.SubType})", LogLevel.Error);
                    return;
                }

                if (string.IsNullOrEmpty(payload))
                {
                    WriteLog("UpdateDeviceBrightness - Payload is empty", LogLevel.Error);
                    return;
                }

                if (int.TryParse(payload, out int brightness) && brightness >= 0 && brightness <= 255)
                {
                    // Check if brightness is already the current value (anti-loop)
                    if (!string.IsNullOrEmpty(dimmer.Bright) && int.TryParse(dimmer.Bright, out int currentBright) && currentBright == brightness)
                    {
                        bool requestedOn = brightness > 0;
                        bool alreadyInRequestedState = requestedOn ? device.Status != "0" : device.Status == "0";
                        if (alreadyInRequestedState)
                        {
                            WriteLog($"[SKIP] Device {device.ID}: Brightness already {brightness}, skipping duplicate command", LogLevel.Debug);
                            return;
                        }
                    }

                    dimmer.Bright = brightness.ToString();
                    device.Status = brightness > 0 ? "1" : "0";
                    string newCommand = BuildDimmerCommand(SessionToken, device.ID, brightness);

                    // Check if this is the same command we just sent (prevent loop)
                    if (newCommand == LastCommand)
                    {
                        WriteLog($"[LOOP-DETECT] Device {device.ID}: Command loop detected, skipping", LogLevel.Debug);
                        return;
                    }

                    LastCommand = newCommand;
                    WriteLog($"Comelit DimmerLight Brightness: status={device.Status}, brightness={brightness} -> Publishing to Comelit");
                    MQTTClient.PublishAsync(new MqttApplicationMessage() { Topic = PublishTopic, PayloadSegment = Encoding.ASCII.GetBytes(LastCommand) });
                    MQTTHomeAssistant?.Publish($"home/lights/{device.GetIDForTopic()}/brightness/state", brightness.ToString());
                    MQTTHomeAssistant?.Publish(device.StatusTopic, brightness > 0 ? "ON" : "OFF");
                }
                else
                {
                    WriteLog($"UpdateDeviceBrightness - Invalid brightness value '{payload}'. Expected 0-255", LogLevel.Warning);
                }
            }
            catch (Exception ex)
            {
                WriteLog($"UpdateDeviceBrightness - Exception: {ex.Message}", LogLevel.Error);
            }
        }

        internal void UpdateDevicePosition(Device device, string payload)
        {
            try
            {
                if (device == null)
                {
                    WriteLog("UpdateDevicePosition - Device is null", LogLevel.Error);
                    return;
                }

                if (device is not ElectricBlind blind)
                {
                    WriteLog($"UpdateDevicePosition - Device '{device.ID}' is not an ElectricBlind (Type: {device.SubType})", LogLevel.Error);
                    return;
                }

                if (string.IsNullOrEmpty(payload))
                {
                    WriteLog("UpdateDevicePosition - Payload is empty", LogLevel.Error);
                    return;
                }

                if (int.TryParse(payload, out int position) && position >= 0 && position <= 100)
                {
                    // Check if position is already the current value (anti-loop)
                    if (!string.IsNullOrEmpty(blind.OpenStatus) && int.TryParse(blind.OpenStatus, out int currentPos) && currentPos == position)
                    {
                        WriteLog($"[SKIP] Device {device.ID}: Position already {position}%, skipping duplicate command", LogLevel.Debug);
                        return;
                    }

                    blind.OpenStatus = position.ToString();
                    string newCommand = BuildBlindPositionCommand(SessionToken, device.ID, position);

                    // Check if this is the same command we just sent (prevent loop)
                    if (newCommand == LastCommand)
                    {
                        WriteLog($"[LOOP-DETECT] Device {device.ID}: Command loop detected, skipping", LogLevel.Debug);
                        return;
                    }

                    LastCommand = newCommand;
                    WriteLog($"Comelit ElectricBlind Position: {position}% -> Publishing to Comelit");
                    MQTTClient.PublishAsync(new MqttApplicationMessage() { Topic = PublishTopic, PayloadSegment = Encoding.ASCII.GetBytes(LastCommand) });
                }
                else
                {
                    WriteLog($"UpdateDevicePosition - Invalid position value '{payload}'. Expected 0-100", LogLevel.Warning);
                }
            }
            catch (Exception ex)
            {
                WriteLog($"UpdateDevicePosition - Exception: {ex.Message}", LogLevel.Error);
            }
        }

        internal void UpdateDeviceTemperature(Device device, string payload)
        {
            try
            {
                if (device == null)
                {
                    WriteLog("UpdateDeviceTemperature - Device is null", LogLevel.Error);
                    return;
                }

                if (!(device is Clima clima))
                {
                    WriteLog($"UpdateDeviceTemperature - Device '{device.ID}' is not a Clima (Type: {device.SubType})", LogLevel.Error);
                    return;
                }

                if (string.IsNullOrEmpty(payload))
                {
                    WriteLog("UpdateDeviceTemperature - Payload is empty", LogLevel.Error);
                    return;
                }

                if (double.TryParse(payload, System.Globalization.CultureInfo.InvariantCulture, out double temperature) && temperature >= 15 && temperature <= 30)
                {
                    string requestedSetpoint = ToComelitTemperatureValue(temperature);

                    if (!string.IsNullOrEmpty(clima.SogliaAttiva) && double.TryParse(NormalizeComelitTemperature(clima.SogliaAttiva), System.Globalization.CultureInfo.InvariantCulture, out double currentSetpoint))
                    {
                        if (Math.Abs(currentSetpoint - temperature) < 0.1)
                        {
                            WriteLog($"[SKIP] Device {device.ID}: Target temperature already {temperature}°C, skipping duplicate command", LogLevel.Debug);
                            return;
                        }
                    }

                    clima.SogliaAttiva = requestedSetpoint;
                    string newCommand = BuildClimaTemperatureCommand(SessionToken, device.ID, temperature);

                    // Check if this is the same command we just sent (prevent loop)
                    if (newCommand == LastCommand)
                    {
                        WriteLog($"[LOOP-DETECT] Device {device.ID}: Command loop detected, skipping", LogLevel.Debug);
                        return;
                    }

                    LastCommand = newCommand;
                    WriteLog($"Comelit Clima Target Temperature: {temperature:F1}°C ({requestedSetpoint}) -> Publishing to Comelit");
                    MQTTClient.PublishAsync(new MqttApplicationMessage() { Topic = PublishTopic, PayloadSegment = Encoding.ASCII.GetBytes(LastCommand) });
                    PublishClimateState(clima);
                }
                else
                {
                    WriteLog($"UpdateDeviceTemperature - Invalid temperature value '{payload}'. Expected 15-30°C", LogLevel.Warning);
                }
            }
            catch (Exception ex)
            {
                WriteLog($"UpdateDeviceTemperature - Exception: {ex.Message}", LogLevel.Error);
            }
        }

        internal void UpdateDeviceHumidity(Device device, string payload)
        {
            try
            {
                if (device == null)
                {
                    WriteLog("UpdateDeviceHumidity - Device is null", LogLevel.Error);
                    return;
                }

                if (!(device is Clima clima))
                {
                    WriteLog($"UpdateDeviceHumidity - Device '{device.ID}' is not a Clima (Type: {device.SubType})", LogLevel.Error);
                    return;
                }

                if (string.IsNullOrEmpty(payload))
                {
                    WriteLog("UpdateDeviceHumidity - Payload is empty", LogLevel.Error);
                    return;
                }

                if (int.TryParse(payload, out int humidity) && humidity >= 30 && humidity <= 80)
                {
                    // Check if humidity is already the current value (anti-loop)
                    if (!string.IsNullOrEmpty(clima.Umidita) && int.TryParse(clima.Umidita, out int currentHum) && currentHum == humidity)
                    {
                        WriteLog($"[SKIP] Device {device.ID}: Humidity already {humidity}%, skipping duplicate command", LogLevel.Debug);
                        return;
                    }

                    clima.Umidita = humidity.ToString();
                    string newCommand = BuildClimaHumidityCommand(SessionToken, device.ID, humidity);

                    // Check if this is the same command we just sent (prevent loop)
                    if (newCommand == LastCommand)
                    {
                        WriteLog($"[LOOP-DETECT] Device {device.ID}: Command loop detected, skipping", LogLevel.Debug);
                        return;
                    }

                    LastCommand = newCommand;
                    WriteLog($"Comelit Clima Humidity: {humidity}% -> Publishing to Comelit");
                    MQTTClient.PublishAsync(new MqttApplicationMessage() { Topic = PublishTopic, PayloadSegment = Encoding.ASCII.GetBytes(LastCommand) });
                }
                else
                {
                    WriteLog($"UpdateDeviceHumidity - Invalid humidity value '{payload}'. Expected 30-80%", LogLevel.Warning);
                }
            }
            catch (Exception ex)
            {
                WriteLog($"UpdateDeviceHumidity - Exception: {ex.Message}", LogLevel.Error);
            }
        }

        internal void PublishClimateState(Clima clima)
        {
            PublishClimateDerivedState(clima);
        }

        private void PublishClimateDerivedState(Clima clima)
        {
            if (MQTTHomeAssistant == null || clima == null)
            {
                return;
            }

            string id = clima.GetIDForTopic();

            if (!string.IsNullOrEmpty(clima.SogliaAttiva))
            {
                MQTTHomeAssistant.Publish($"home/climate/{id}/target-temperature/state", NormalizeComelitTemperature(clima.SogliaAttiva));
            }

            if (!string.IsNullOrEmpty(clima.SogliaAttivaUmi))
            {
                MQTTHomeAssistant.Publish($"home/climate/{id}/target-humidity/state", clima.SogliaAttivaUmi);
            }

            MQTTHomeAssistant.Publish($"home/climate/{id}/mode/state", GetClimateMode(clima));
            MQTTHomeAssistant.Publish($"home/climate/{id}/action/state", GetClimateAction(clima));
        }

        private static string GetClimateMode(Clima clima)
        {
            if (IsClimateOff(clima))
            {
                return "off";
            }

            return clima.EstInv switch
            {
                "0" => "heat",
                "1" => "cool",
                _ => "heat"
            };
        }

        private static string GetClimateAction(Clima clima)
        {
            if (IsClimateOff(clima))
            {
                return "off";
            }

            if (clima.Status == "16")
            {
                return clima.EstInv == "1" ? "cooling" : "heating";
            }

            return "idle";
        }

        private static bool IsClimateOff(Clima clima)
        {
            return clima.PowerSt == "0" || clima.AutoMan == "6";
        }

        private Device CreateDeviceBinding(ElementData elData, string area = "")
        {
            Device output = null;
            switch (elData.Type)
            {
                case Enums.OBJECT_TYPE.LIGHT:
                    output = CreateLightBinding(elData, area);
                    break;
                case Enums.OBJECT_TYPE.BLIND:
                    output = CreateBlindBinding(elData, area);
                    break;
                case Enums.OBJECT_TYPE.OTHER:
                    output = CreateOtherBinding(elData, area);
                    break;
                case Enums.OBJECT_TYPE.AUTOMATION:
                    output = CreateAutomationBinding(elData, area);
                    break;
                case Enums.OBJECT_TYPE.IRRIGATION:
                    output = CreateIrrigationBinding(elData, area);
                    break;
                case Enums.OBJECT_TYPE.POWER_SUPPLIER:
                case Enums.OBJECT_TYPE.OUTLET:
                    output = CreateSensorBinding(elData, area);
                    break;
                case Enums.OBJECT_TYPE.THERMOSTAT:
                    output = CreateClimaBinding(elData, area);
                    break;
                case Enums.OBJECT_TYPE.INPUT:
                    output = CreateInputBinding(elData, area);
                    break;
                case Enums.OBJECT_TYPE.RULE:
                    if (IsAlarmRule(elData))
                    {
                        output = CreateAlarmBinding(elData, area);
                    }
                    break;
            }
            return output;
        }

        private Device CreateAlarmBinding(ElementData elData, string area)
        {
            Input input = JsonConvert.DeserializeObject<Input>(JsonConvert.SerializeObject(elData));
            input.AreaName = area;
            Devices.CreateAlarmConfig(input);
            Device alarm = input;
            //MQTTClient.PublishAsync(new MqttApplicationMessage() { Topic = PublishTopic, PayloadSegment = Encoding.ASCII.GetBytes(Subscribe.Replace("#sessionToken#", SessionToken).Replace("#objID#", elData.ID)) });
            return alarm;
        }

        private Device CreateAutomationBinding(ElementData elData, string area)
        {
            Automation automation = JsonConvert.DeserializeObject<Automation>(JsonConvert.SerializeObject(elData));
            automation.AreaName = area;
            Devices.CreateAutomationConfig(automation);
            return automation;
        }

        private Device CreateSensorBinding(ElementData elData, string area)
        {
            ComelitSensor sensor = JsonConvert.DeserializeObject<ComelitSensor>(JsonConvert.SerializeObject(elData));
            sensor.AreaName = area;
            sensor.StateClass = "measurement";
            sensor.Icon = "mdi:counter";

            if (string.Equals(sensor.LabelValue, "l", StringComparison.OrdinalIgnoreCase))
            {
                sensor.DeviceClass = "water";
                sensor.UnitOfMeasurement = "L";
                sensor.Icon = "mdi:water";
            }
            else if (elData.Type == Enums.OBJECT_TYPE.POWER_SUPPLIER || !string.IsNullOrWhiteSpace(sensor.InstantPower))
            {
                sensor.DeviceClass = "power";
                sensor.UnitOfMeasurement = "W";
                sensor.Icon = "mdi:flash";
            }
            else
            {
                sensor.UnitOfMeasurement = sensor.LabelValue;
            }

            Devices.CreateSensorConfig(sensor);
            return sensor;
        }

        private Device CreateInputBinding(ElementData elData, string area)
        {
            Input input = JsonConvert.DeserializeObject<Input>(JsonConvert.SerializeObject(elData));
            input.AreaName = area;
            Devices.CreateInputConfig(input);
            Device output = input;
            //MQTTClient.PublishAsync(new MqttApplicationMessage() { Topic = PublishTopic, PayloadSegment = Encoding.ASCII.GetBytes(Subscribe.Replace("#sessionToken#", SessionToken).Replace("#objID#", elData.ID)) });
            return output;
        }

        private Device CreateClimaBinding(ElementData elData, string area)
        {
            Device output = null;
            switch (elData.SubType)
            {
                case Enums.OBJECT_SUBTYPE.CLIMA_THERMOSTAT_DEHUMIDIFIER:
                    Clima clima = JsonConvert.DeserializeObject<Clima>(JsonConvert.SerializeObject(elData));
                    clima.AreaName = area;
                    Devices.CreateClimaConfig(clima);
                    output = clima;
                    //MQTTClient.PublishAsync(new MqttApplicationMessage() { Topic = PublishTopic, PayloadSegment = Encoding.ASCII.GetBytes(Subscribe.Replace("#sessionToken#", SessionToken).Replace("#objID#", elData.ID)) });
                    break;
            }
            return output;
        }

        private Device CreateIrrigationBinding(ElementData elData, string area)
        {
            Device output = null;
            switch (elData.SubType)
            {
                case Enums.OBJECT_SUBTYPE.IRRIGATION_VALVE:
                    IrrigationValve valve = JsonConvert.DeserializeObject<IrrigationValve>(JsonConvert.SerializeObject(elData));
                    valve.AreaName = area;
                    Devices.CreateIrrigationValveConfig(valve);
                    output = valve;
                    //MQTTClient.PublishAsync(new MqttApplicationMessage() { Topic = PublishTopic, PayloadSegment = Encoding.ASCII.GetBytes(Subscribe.Replace("#sessionToken#", SessionToken).Replace("#objID#", elData.ID)) });
                    break;
            }
            return output;
        }

        private Device CreateOtherBinding(ElementData elData, string area)
        {
            Device output = null;
            switch (elData.SubType)
            {
                case Enums.OBJECT_SUBTYPE.OTHER_DIGIT:
                    OtherDigit other = JsonConvert.DeserializeObject<OtherDigit>(JsonConvert.SerializeObject(elData));
                    other.AreaName = area;
                    Devices.CreateOtherDigitConfig(other);
                    output = other;
                    //MQTTClient.PublishAsync(new MqttApplicationMessage() { Topic = PublishTopic, PayloadSegment = Encoding.ASCII.GetBytes(Subscribe.Replace("#sessionToken#", SessionToken).Replace("#objID#", elData.ID)) });
                    break;
                case Enums.OBJECT_SUBTYPE.OTHER_TMP:
                    OtherTemporary temporary = JsonConvert.DeserializeObject<OtherTemporary>(JsonConvert.SerializeObject(elData));
                    temporary.AreaName = area;
                    Devices.CreateOtherTemporaryConfig(temporary);
                    output = temporary;
                    break;
            }
            return output;
        }

        private Device CreateLightBinding(ElementData elData, string area)
        {
            Device output = null;
            switch (elData.SubType)
            {
                case Enums.OBJECT_SUBTYPE.DIGITAL_LIGHT:
                    //Subscribe to status changed event
                    DigitalLight light = JsonConvert.DeserializeObject<DigitalLight>(JsonConvert.SerializeObject(elData));
                    light.AreaName = area;
                    Devices.CreateDigitalLightConfig(light);
                    output = light;
                    //MQTTClient.PublishAsync(new MqttApplicationMessage() { Topic = PublishTopic, PayloadSegment = Encoding.ASCII.GetBytes(Subscribe.Replace("#sessionToken#", SessionToken).Replace("#objID#", elData.ID)) });
                    break;

                case Enums.OBJECT_SUBTYPE.RGB_LIGHT: break;

                case Enums.OBJECT_SUBTYPE.TEMPORIZED_LIGHT: break;

                case Enums.OBJECT_SUBTYPE.DIMMER_LIGHT:
                    DimmerLight dimmer = JsonConvert.DeserializeObject<DimmerLight>(JsonConvert.SerializeObject(elData));
                    dimmer.AreaName = area;
                    Devices.CreateDimmerLightConfig(dimmer);
                    output = dimmer;
                    //MQTTClient.PublishAsync(new MqttApplicationMessage() { Topic = PublishTopic, PayloadSegment = Encoding.ASCII.GetBytes(Subscribe.Replace("#sessionToken#", SessionToken).Replace("#objID#", elData.ID)) });
                    break;
            }
            return output;
        }

        private Device CreateBlindBinding(ElementData elData, string area)
        {
            Device output = null;
            switch (elData.SubType)
            {
                case Enums.OBJECT_SUBTYPE.ELECTRIC_BLIND:
                case Enums.OBJECT_SUBTYPE.ENHANCED_ELECTRIC_BLIND:
                    ElectricBlind blind = JsonConvert.DeserializeObject<ElectricBlind>(JsonConvert.SerializeObject(elData));
                    blind.AreaName = area;
                    Devices.CreateElectricBlindConfig(blind);
                    output = blind;
                    //MQTTClient.PublishAsync(new MqttApplicationMessage() { Topic = PublishTopic, PayloadSegment = Encoding.ASCII.GetBytes(Subscribe.Replace("#sessionToken#", SessionToken).Replace("#objID#", elData.ID)) });
                    break;
            }
            return output;
        }

        private void SubscribeToDeviceValueChange()
        {
            foreach (Area area in HomeStructure.Areas)
            {
                foreach (Device device in area.Devices)
                {
                    if (device != null && device.CanSubscribe)
                    {
                        MQTTClient.PublishAsync(new MqttApplicationMessage() { Topic = PublishTopic, PayloadSegment = Encoding.ASCII.GetBytes(BuildSubscribe(SessionToken, device.ID)) });
                    }
                }
            }
        }
    }
}


//ESEMPIO DI RICHIESTA STATUS LUCE
//string s = "{\"req_type\": 0,\"seq_id\": 4,\"req_sub_type\": -1,\"sessiontoken\": \"#sessionToken#\",\"obj_id\": \"#objID#\",\"detail_level\": 1}";
//ElementData light = Devices.Lights.FirstOrDefault(l => l.Descrizione == "Garage");
//s = s.Replace("#sessionToken#", SessionToken).Replace("#objID#", light.ID);
//MQTTClient.PublishAsync(new MqttApplicationMessage() { Topic = PublishTopic, PayloadSegment = Encoding.ASCII.GetBytes(s) });
