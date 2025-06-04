using MQTT_NET_COMELIT.Comelit.DevicesStructure;
using System.Text;
using static MQTT_NET_COMELIT.Utility.Utility;

namespace MQTT_NET_COMELIT.Comelit
{
    public partial class MQTTComelit
    {
        private readonly int _pollingMs;
        public const string PollingStatus = $"home/inputs/comelit-available/state";
        public string PollingConfigPayload = "";
        public string PollingConfigTopic = $"homeassistant/binary_sensor/comelit-available/config";
        public bool IsPolling = false;

        private DigitalLight pollingDevice;
        public DigitalLight PollingDevice
        {
            get { return pollingDevice; }
            set
            {
                pollingDevice = value;
                if (pollingDevice != null)
                {
                    PollingConfigPayload = new MQTTConfig() { name = "Comelit Integration Service", object_id = pollingDevice.ID, off_delay = $"{_pollingMs / 1000 * 1.5}", unique_id = pollingDevice.ID, state_topic = PollingStatus, device = new MQTTDevice() { identifiers = "", manufacturer = "Ivan", model = "Comelit Integration", name = "Comelit service available", suggested_area = "Sistema" } }.ToString();
                }
            }
        }

        public void StartPolling()
        {
            Task.Run(async () =>
            {
                while (ConnectedAndLoggedIn)
                {
                    WriteLog($"Polling digital light {PollingDevice.ID}");
                    IsPolling = true;
                    MQTTClient.PublishAsync(new MQTTnet.MqttApplicationMessage() { Topic = PublishTopic, PayloadSegment = Encoding.ASCII.GetBytes(Status.Replace("#sessionToken#", SessionToken).Replace("#OBJID#", PollingDevice.ID)) });
                    await Task.Delay(_pollingMs);
                }
            });
        }

    }
}
