using MQTT_NET_COMELIT.Comelit;
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
            Assert.Equal("room_light_1", result);
        }
    }
}
