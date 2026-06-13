using MQTT_NET_COMELIT.Comelit;
using System.Text.Json;
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

        [Fact]
        public void CreateClimaConfig_AddsHumidityHistorySensors()
        {
            var device = new Clima
            {
                ID = "DOM#CL#17#1",
                Description = "Termostato",
                AreaName = "Zona giorno"
            };

            Devices.CreateClimaConfig(device);

            Assert.True(device.ExtraConfigPayloads.TryGetValue("homeassistant/sensor/dom_cl_17_1_current_humidity/config", out string currentHumidityConfig));
            Assert.True(device.ExtraConfigPayloads.TryGetValue("homeassistant/sensor/dom_cl_17_1_target_humidity/config", out string targetHumidityConfig));

            using JsonDocument currentHumidity = JsonDocument.Parse(currentHumidityConfig);
            using JsonDocument targetHumidity = JsonDocument.Parse(targetHumidityConfig);

            Assert.Equal("home/climate/dom_cl_17_1/current-humidity/state", currentHumidity.RootElement.GetProperty("state_topic").GetString());
            Assert.Equal("humidity", currentHumidity.RootElement.GetProperty("device_class").GetString());
            Assert.Equal("%", currentHumidity.RootElement.GetProperty("unit_of_measurement").GetString());
            Assert.Equal("measurement", currentHumidity.RootElement.GetProperty("state_class").GetString());

            Assert.Equal("home/climate/dom_cl_17_1/target-humidity/state", targetHumidity.RootElement.GetProperty("state_topic").GetString());
            Assert.Equal("humidity", targetHumidity.RootElement.GetProperty("device_class").GetString());
            Assert.Equal("%", targetHumidity.RootElement.GetProperty("unit_of_measurement").GetString());
            Assert.Equal("measurement", targetHumidity.RootElement.GetProperty("state_class").GetString());
        }
    }
}
