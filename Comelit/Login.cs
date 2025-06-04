using MQTT_NET_COMELIT.Comelit.DevicesStructure;
using MQTTnet;
using Newtonsoft.Json;
using System.Text;
using static MQTT_NET_COMELIT.Comelit.JsonParsing;
using static MQTT_NET_COMELIT.Utility.Utility;

namespace MQTT_NET_COMELIT.Comelit
{
    public partial class MQTTComelit
    {
        public bool ConnectedAndLoggedIn { get { return MQTTConnected & MQTTLoggedIn; } }
        public bool MQTTConnected { get; private set; }
        public bool MQTTLoggedIn { get; private set; }

        private void LoginSequence(string payload = null)
        {
            MQTTLoggedIn = false;
            PollingDevice = null;
            WriteLog($"Login sequence: {payload}");
            if (payload == null)
            {
                WriteLog("Publishing to topic " + SubscribeTopic + " the ANNUNCE payload");
                MQTTClient.PublishAsync(new MqttApplicationMessage() { Topic = PublishTopic, PayloadSegment = Encoding.ASCII.GetBytes(Annunce) });
            }
            else
            {
                Header GetStep = JsonConvert.DeserializeObject<Header>(payload);
                if (GetStep != null)
                {
                    if (GetStep.SeqID != null)
                    {
                        WriteLog("Decoding message with sequence " + int.Parse(GetStep.SeqID.ToString()));
                        switch (int.Parse(GetStep.SeqID.ToString()))
                        {
                            case 1:
                                Header GetAgent = JsonConvert.DeserializeObject<Header>(payload);
                                Agent = GetAgent.OutData.FirstOrDefault().AgentID;
                                WriteLog("Got AgentID. Starting with LOGIN payload");
                                MQTTClient.PublishAsync(new MqttApplicationMessage() { Topic = PublishTopic, PayloadSegment = Encoding.ASCII.GetBytes(Login.Replace("#agentID#", Agent)) });
                                break;

                            case 2:
                                Header GetToken = JsonConvert.DeserializeObject<Header>(payload);
                                SessionToken = GetToken.SessionToken;
                                WriteLog("Got SessionToken. Starting with STATUS payload");
                                MQTTClient.PublishAsync(new MqttApplicationMessage() { Topic = PublishTopic, PayloadSegment = Encoding.ASCII.GetBytes(Status.Replace("#sessionToken#", SessionToken).Replace("#OBJID#", ComelitROOTElement)) });
                                break;

                            case 3:
                                Header ROOT = JsonConvert.DeserializeObject<Header>(payload);
                                MQTTLoggedIn = ROOT != null;
                                if (HomeStructure == null)
                                {
                                    WriteLog("Create structure and subscribe to value change event");
                                    CreateStructure(ROOT);
                                    SubscribeToDeviceValueChange();
                                    if (MQTTLoggedIn) WriteLog("Comelit devices list updated!");
                                }
                                foreach (Area a in HomeStructure.Areas)
                                {
                                    foreach (Device dev in a.Devices)
                                    {
                                        if (dev.GetType() == typeof(DigitalLight))
                                        {
                                            dev.IsPollingDevice = true;
                                            PollingDevice = (DigitalLight)dev;
                                        }
                                        if (PollingDevice != null) break;
                                    }
                                    if (PollingDevice != null) break;
                                }                                
                                break;

                            default:
                                WriteLog($"Communication sequence step {int.Parse(GetStep.SeqID.ToString())} not implemented yet! Message: {payload}");
                                break;
                        }
                    }
                }
            }
            WriteLog($"Login sequence END - Result: {MQTTLoggedIn}");
        }
    }
}
