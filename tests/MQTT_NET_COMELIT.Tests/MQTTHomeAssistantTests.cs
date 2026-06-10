using System.Reflection;
using System.Runtime.Serialization;
using MQTT_NET_COMELIT.Comelit;
using MQTT_NET_COMELIT.HomeAssistant;
using Xunit;
using System.Collections.Generic;

namespace MQTT_NET_COMELIT.Tests
{
    public class MQTTHomeAssistantTests
    {
        [Fact]
        public void GetDeviceFromTopic_ReturnsCorrectDevice()
        {
            var device = new DigitalLight { ID = "room#light.1" };
            var area = new Area { Devices = new List<Device> { device } };
            var home = new Home { Areas = new List<Area> { area } };

            var comelit = (MQTTComelit)FormatterServices.GetUninitializedObject(typeof(MQTTComelit));
            typeof(MQTTComelit).GetField("<HomeStructure>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(comelit, home);

            var ha = (MQTTHomeAssistant)FormatterServices.GetUninitializedObject(typeof(MQTTHomeAssistant));
            ha.MQTTComelit = comelit;
            typeof(MQTTHomeAssistant).GetField("_deviceCache", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(ha, new Dictionary<string, Device>());

            var method = typeof(MQTTHomeAssistant).GetMethod("GetDeviceFromTopic", BindingFlags.Instance | BindingFlags.NonPublic);
            var result = (Device)method.Invoke(ha, new object[] { $"home/lights/{device.GetIDForTopic()}/set" });

            Assert.Same(device, result);
        }
    }
}
