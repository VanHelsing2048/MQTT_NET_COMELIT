using System.Text;
using static MQTT_NET_COMELIT.Comelit.ComelitMessages;
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
        private bool _pollingStarted = false;

        private DigitalLight pollingDevice;
        public DigitalLight PollingDevice
        {
            get { return pollingDevice; }
            set
            {
                pollingDevice = value;
                if (pollingDevice != null)
                {
                    PollingConfigPayload = new MQTTConfig() { Name = "Comelit Integration Service", ObjectId = pollingDevice.ID, OffDelay = (int)(_pollingMs / 1000 * 1.5), UniqueId = pollingDevice.ID, StateTopic = PollingStatus, Device = new MQTTDevice() { Identifiers = ["ServiceAvailable"], Manufacturer = "Ivan", Model = "Comelit Integration", Name = "Comelit service available", SuggestedArea = "Sistema" } }.ToString();
                }
            }
        }

        public void StartPolling()
        {
            if (_pollingStarted)
            {
                return;
            }

            _pollingStarted = true;
            Task.Run(async () =>
            {
                while (true)
                {
                    if (ConnectedAndLoggedIn && PollingDevice != null)
                    {
                        WriteLog($"Polling digital light {PollingDevice.ID}", LogLevel.Debug);
                        IsPolling = true;
                        await MQTTClient.PublishAsync(new MQTTnet.MqttApplicationMessage() { Topic = PublishTopic, PayloadSegment = Encoding.ASCII.GetBytes(BuildStatus(SessionToken, pollingDevice.ID)) });
                    }

                    await Task.Delay(_pollingMs);
                }
            });
        }

    }
}
