using MQTT_NET_COMELIT.Comelit;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;
using System.Text;
using static MQTT_NET_COMELIT.Utility.Utility;
using MQTT_NET_COMELIT.Comelit.DevicesStructure;

namespace MQTT_NET_COMELIT.HomeAssistant
{
    //Username = mosquitto
    //Password = raspberry
    //Ip = 192.168.1.63
    public class MQTTHomeAssistant
    {
        public bool ConnectedAndLoggedIn { get; private set; }
        public MQTTComelit MQTTComelit { get; set; }

        readonly string MQTTUsername;
        readonly string MQTTPassword;
        readonly string MQTTIPAddress;

        IMqttClient MQTTClient;

        public MQTTHomeAssistant(string username, string password, string ip, MQTTComelit mQTTComelit)
        {
            MQTTUsername = username;
            MQTTPassword = password;
            MQTTIPAddress = ip;
            MQTTComelit = mQTTComelit;
            StartMQTT();
        }

        private async Task StartMQTT()
        {
            try
            {
                using (MQTTClient = new MqttFactory().CreateMqttClient())
                {
                    var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(MQTTIPAddress).WithCredentials(MQTTUsername, MQTTPassword).Build();
                    var timeout = new CancellationTokenSource(5000);
                    MQTTClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedAsync;
                    MQTTClient.ConnectedAsync += MqttClient_ConnectedAsync;
                    var response = await MQTTClient.ConnectAsync(mqttClientOptions, timeout.Token);
                    while (MQTTClient.IsConnected) { await Task.Delay(1000); }
                    WriteLog("The HomeAssistant MQTT client is disconnected.");
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.ToString());
            }
            Console.ReadLine();
        }

        private Task MqttClient_ConnectedAsync(MqttClientConnectedEventArgs args)
        {
            WriteLog("The HomeAssistant MQTT client is connected.");
            foreach (Area area in MQTTComelit.HomeStructure.Areas)
            {
                foreach (Device device in area.Devices)
                {
                    switch (device?.SubType)
                    {
                        case Enums.OBJECT_SUBTYPE.DIGITAL_LIGHT:
                            MQTTClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(device.CommandTopic).Build());
                            break;
                        case Enums.OBJECT_SUBTYPE.IRRIGATION_VALVE:
                            MQTTClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(device.CommandTopic).Build());
                            break;
                        case Enums.OBJECT_SUBTYPE.OTHER_DIGIT:
                            MQTTClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(device.CommandTopic).Build());
                            break;
                    }
                }
            }
            //MQTTClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic($"#").Build());
            ConnectedAndLoggedIn = true;
            return Task.CompletedTask;
        }

        private Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
        {
            WriteLog("HomeAssistant NewMessage: " + args.ApplicationMessage.Topic + " - " + Encoding.Default.GetString(args.ApplicationMessage.PayloadSegment));
            Device device = GetDeviceFromTopic(args.ApplicationMessage.Topic);
            string payload = Encoding.Default.GetString(args.ApplicationMessage.PayloadSegment);
            MQTTComelit.UpdateDevice(device, payload);            
            return Task.CompletedTask;
        }

        private Device GetDeviceFromTopic(string topic)
        {
            foreach (Area area in MQTTComelit.HomeStructure.Areas)
            {
                foreach (Device dev in area.Devices)
                {
                    if (dev != null && topic.Contains(dev.GetIDForTopic())) { return dev; }
                }
            }
            return null;
        }

        public void Publish(string topic, string payload, bool retain = false)
        {
            if (payload == "1") payload = "ON";
            if (payload == "0") payload = "OFF";
            WriteLog("HomeAssistant Publish: " + topic + " - " + payload);
            MQTTClient.PublishAsync(new MqttApplicationMessage() { Topic = topic, PayloadSegment = Encoding.ASCII.GetBytes(payload), Retain = retain });
        }
    }
}
