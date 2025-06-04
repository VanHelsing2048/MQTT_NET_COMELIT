using MQTT_NET_COMELIT.Comelit.DevicesStructure;
using Xunit;

namespace MQTT_NET_COMELIT.Tests
{
    public class DeviceTests
    {
        [Fact]
        public void GetIDForTopic_ReplacesHashAndDot()
        {
            var device = new DigitalLight { ID = "room#light.1" };
            var result = device.GetIDForTopic();
            Assert.Equal("roomhashlightdot1", result);
        }
    }
}
