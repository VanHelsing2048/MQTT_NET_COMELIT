using MQTTnet;
using MQTTnet.Client;
using System.Text;
using static MQTT_NET_COMELIT.Utility.Utility;
using static MQTT_NET_COMELIT.Comelit.ComelitMessages;

namespace MQTT_NET_COMELIT.Comelit
{
    // hsrv-user|sf1nE9bjPc|ipc-user|irj6Glv6J0
    //user hsrv-user
    //pass sf1nE9bjPc

    //const string HubMACAddress = "00252917071D";
    //const string MQTTUsername = "hsrv-user";
    //const string MQTTPassword = "sf1nE9bjPc";
    //const string ComelitROOTElement = "GEN#17#13#1";
    //const string ServerIPAddress = "192.168.1.51";

    public partial class MQTTComelit
    {
        readonly string HubMACAddress;
        readonly string MQTTUsername;
        readonly string MQTTPassword;
        readonly string ComelitROOTElement;
        readonly string ServerIPAddress;

        readonly string MQTTClientID = $"HSrv_{Guid.NewGuid().ToString().ToUpper()}";
        readonly string SubscribeTopic;
        readonly string PublishTopic;

        IMqttClient MQTTClient;

        string Agent = string.Empty;
        string SessionToken = string.Empty;


        private async Task StartMQTT()
        {
            try
            {
                using (MQTTClient = new MqttFactory().CreateMqttClient())
                {
                    var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(ServerIPAddress).WithCredentials(MQTTUsername, MQTTPassword).WithClientId(MQTTClientID).Build();
                    var timeout = new CancellationTokenSource(5000);
                    MQTTClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedAsync;
                    MQTTClient.ConnectedAsync += MqttClient_ConnectedAsync;
                    var response = await MQTTClient.ConnectAsync(mqttClientOptions, timeout.Token);
                    while (MQTTClient.IsConnected) { await Task.Delay(1000); }
                    WriteLog("The Comelit MQTT client is disconnected.");
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.ToString());
            }
        }

        private Task MqttClient_ConnectedAsync(MqttClientConnectedEventArgs arg)
        {
            MQTTConnected = true;
            WriteLog("The Comelit MQTT client is connected.");
            WriteLog("Subscribing to topic: " + SubscribeTopic);
            MQTTClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(SubscribeTopic).Build());
            //Inizio la sequenza per richedere il SessionToken e l'AgentID
            WriteLog("Starting connection sequence.");
            LoginSequence();
            return Task.CompletedTask;
        }

        private Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            MessageManager(Encoding.Default.GetString(arg.ApplicationMessage.PayloadSegment));
            return Task.CompletedTask;
        }
    }
}
