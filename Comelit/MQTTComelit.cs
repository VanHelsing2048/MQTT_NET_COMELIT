using MQTT_NET_COMELIT.Comelit.DevicesStructure;
using MQTT_NET_COMELIT.HomeAssistant;
using MQTTnet;
using Newtonsoft.Json;
using System.Text;
using System.Xml.Linq;
using static MQTT_NET_COMELIT.Comelit.JsonParsing;
using static MQTT_NET_COMELIT.Utility.Utility;
using static MQTT_NET_COMELIT.Comelit.ComelitMessages;

namespace MQTT_NET_COMELIT.Comelit
{
    public partial class MQTTComelit
    {
        public MQTTHomeAssistant MQTTHomeAssistant { get; set; }
        private string LastCommand = string.Empty;


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

            StartMQTT();
        }

        private void MessageManager(string payload)
        {
            if (!MQTTLoggedIn) { LoginSequence(payload); } else { ManageNewMessage(payload); }
        }

        private void ManageNewMessage(string message)
        {
            WriteLog($"COMELIT NEW MESSAGE: {message}");
            Header GetStep = JsonConvert.DeserializeObject<Header>(message);
            if (GetStep != null)
            {
                if (GetStep.ObjID != null && ConnectedAndLoggedIn && GetStep.Message == null)
                {
                    OutData recDev = GetStep.OutData.First();

                    foreach (Area area in HomeStructure.Areas)
                    {
                        Device device = area.Devices.FirstOrDefault(d => d?.ID == recDev.ID);
                        if (device != null)
                        {
                            switch (device.SubType)
                            {
                                case Enums.OBJECT_SUBTYPE.DIGITAL_LIGHT:
                                    device.Status = recDev.Status;
                                    MQTTHomeAssistant.Publish(device.StatusTopic, device.StatusONOFF);
                                    break;
                                case Enums.OBJECT_SUBTYPE.IRRIGATION_VALVE:
                                    device.Status = recDev.Status;
                                    MQTTHomeAssistant.Publish(device.StatusTopic, device.StatusONOFF);
                                    break;
                                case Enums.OBJECT_SUBTYPE.OTHER_DIGIT:
                                    device.Status = recDev.Status;
                                    MQTTHomeAssistant.Publish(device.StatusTopic, device.StatusONOFF);
                                    break;
                            }
                            switch (device.Type)
                            {
                                case Enums.OBJECT_TYPE.INPUT:
                                    device.Status = recDev.Status;
                                    MQTTHomeAssistant.Publish(device?.StatusTopic, device.StatusONOFF);
                                    break;
                                case Enums.OBJECT_TYPE.RULE:
                                    device.Status = recDev.Status;
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
                            WriteLog("Sending LastCommand after LoginSequence: " + LastCommand);
                            MQTTClient.PublishAsync(new MqttApplicationMessage() { Topic = PublishTopic, PayloadSegment = Encoding.ASCII.GetBytes(LastCommand) });
                            break;
                    }
                }
                else if (IsPolling)
                {
                    OutData data = GetStep.OutData[0];
                    if (data.SubType == PollingDevice.SubType && data.ID == PollingDevice.ID)
                    {
                        MQTTHomeAssistant.Publish(PollingStatus, "ON");
                    }
                }
            }
        }

        internal void UpdateDevice(Device device, string payload)
        {
            if (device == null)
            {
                WriteLog("MQTTComelit: UpdateDevice has NULL device");
            }
            else
            {
                switch (device.SubType)
                {
                    case Enums.OBJECT_SUBTYPE.DIGITAL_LIGHT:
                        if (payload == "ON") { device.Status = "1"; }
                        if (payload == "OFF") { device.Status = "0"; }
                        LastCommand = BuildOnOffCommand(SessionToken, device.ID, device.Status);
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
                }
                WriteLog($"Comelit {device.SubType}: {LastCommand}");
                MQTTClient.PublishAsync(new MqttApplicationMessage() { Topic = PublishTopic, PayloadSegment = Encoding.ASCII.GetBytes(LastCommand) });

            }
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
                case Enums.OBJECT_TYPE.IRRIGATION:
                    output = CreateIrrigationBinding(elData, area);
                    break;
                case Enums.OBJECT_TYPE.THERMOSTAT:
                    output = CreateClimaBinding(elData, area);
                    break;
                case Enums.OBJECT_TYPE.INPUT:
                    output = CreateInputBinding(elData, area);
                    break;
                case Enums.OBJECT_TYPE.RULE:
                    output = CreateAlarmBinding(elData, area);
                    break;
            }
            return output;
        }

        private Device CreateAlarmBinding(ElementData elData, string area)
        {
            Device alarm = null;
            Input input = JsonConvert.DeserializeObject<Input>(JsonConvert.SerializeObject(elData));
            input.AreaName = area;
            Devices.CreateAlarmConfig(input);
            alarm = input;
            //MQTTClient.PublishAsync(new MqttApplicationMessage() { Topic = PublishTopic, PayloadSegment = Encoding.ASCII.GetBytes(Subscribe.Replace("#sessionToken#", SessionToken).Replace("#objID#", elData.ID)) });
            return alarm;
        }

        private Device CreateInputBinding(ElementData elData, string area)
        {
            Device output = null;
            Input input = JsonConvert.DeserializeObject<Input>(JsonConvert.SerializeObject(elData));
            input.AreaName = area;
            Devices.CreateInputConfig(input);
            output = input;
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