using MQTTnet;
using MQTTnet.Client;
using System.Text;
using static MQTT_NET_COMELIT.Utility.Utility;

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

        const string Annunce = "{\"req_type\": 13,\"seq_id\": 1,\"req_sub_type\": -1,\"agent_type\": 0}";
        const string Login = "{\"req_type\": 5,\"seq_id\": 2,\"req_sub_type\": -1,\"agent_type\": 0,\"agent_id\": #agentID#,\"user_name\": \"admin\",\"password\": \"admin\"}";
        const string Status = "{\"req_type\": 0,\"seq_id\": 3,\"req_sub_type\": -1,\"sessiontoken\": \"#sessionToken#\",\"obj_id\": \"#OBJID#\",\"detail_level\": 1}"; //ROOT = GEN#17#13#1

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
