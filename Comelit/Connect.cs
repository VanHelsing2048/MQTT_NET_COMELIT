using MQTTnet;
using System.Text;
using static MQTT_NET_COMELIT.Utility.Utility;

namespace MQTT_NET_COMELIT.Comelit
{
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
        Task MQTTTask;

        string Agent = string.Empty;
        string SessionToken = string.Empty;

        private const int MinComelitReconnectDelayMs = 5000;
        private const int MaxComelitReconnectDelayMs = 60000;

        private async Task StartMQTT()
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    using (MQTTClient = new MqttClientFactory().CreateMqttClient())
                    {
                        var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(ServerIPAddress).WithCredentials(MQTTUsername, MQTTPassword).WithClientId(MQTTClientID).Build();
                        using var timeout = new CancellationTokenSource(5000);
                        MQTTClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedAsync;
                        MQTTClient.ConnectedAsync += MqttClient_ConnectedAsync;
                        MQTTClient.DisconnectedAsync += MqttClient_DisconnectedAsync;
                        await MQTTClient.ConnectAsync(mqttClientOptions, timeout.Token);
                        retryCount = 0;
                        while (MQTTClient.IsConnected) { await Task.Delay(1000); }
                        WriteLog("The Comelit MQTT client is disconnected.");
                    }
                }
                catch (Exception ex)
                {
                    WriteLog($"Comelit MQTT connection error: {ex.Message}", LogLevel.Warning);
                }

                MQTTConnected = false;
                MQTTLoggedIn = false;
                MQTTHomeAssistant?.Publish(PollingStatus, "OFF", true);
                Agent = string.Empty;
                SessionToken = string.Empty;
                retryCount++;
                int retryDelayMs = Math.Min(MinComelitReconnectDelayMs * retryCount, MaxComelitReconnectDelayMs);
                WriteLog($"Retrying Comelit MQTT connection (attempt {retryCount}) in {retryDelayMs}ms...");
                await Task.Delay(retryDelayMs);
            }
        }

        private Task MqttClient_ConnectedAsync(MqttClientConnectedEventArgs arg)
        {
            MQTTConnected = true;
            MQTTLoggedIn = false;
            _deviceStateManager.ClearAllSnapshots();
            WriteLog("The Comelit MQTT client is connected.");
            WriteLog("Subscribing to topic: " + SubscribeTopic);
            MQTTClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(SubscribeTopic).Build());
            //Inizio la sequenza per richedere il SessionToken e l'AgentID
            WriteLog("Starting connection sequence.");
            LoginSequence();
            return Task.CompletedTask;
        }

        private Task MqttClient_DisconnectedAsync(MqttClientDisconnectedEventArgs arg)
        {
            MQTTConnected = false;
            MQTTLoggedIn = false;
            MQTTHomeAssistant?.Publish(PollingStatus, "OFF", true);
            WriteLog($"Comelit MQTT client disconnected: {arg.Reason}");
            return Task.CompletedTask;
        }

        private Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            MessageManager(Encoding.Default.GetString(arg.ApplicationMessage.Payload));
            return Task.CompletedTask;
        }
    }
}
