using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using MQTT_NET_COMELIT.Comelit;
using MQTTnet;
using Moq;
using Xunit;

namespace MQTT_NET_COMELIT.Tests
{
    public class MQTTComelitTests
    {
        private MQTTComelit CreateComelitWithClient(Mock<IMqttClient> mock)
        {
            var comelit = (MQTTComelit)FormatterServices.GetUninitializedObject(typeof(MQTTComelit));
            typeof(MQTTComelit).GetField("PublishTopic", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(comelit, "topic");
            typeof(MQTTComelit).GetField("SessionToken", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(comelit, "token");
            typeof(MQTTComelit).GetField("MQTTClient", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(comelit, mock.Object);
            return comelit;
        }

        [Fact]
        public void UpdateDevice_NullDevice_LogsWarning()
        {
            var mockClient = new Mock<IMqttClient>();
            var comelit = CreateComelitWithClient(mockClient);

            var sw = new StringWriter();
            Console.SetOut(sw);

            var method = typeof(MQTTComelit).GetMethod("UpdateDevice", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(comelit, new object?[] { null, "ON" });

            Assert.Contains("UpdateDevice has NULL device", sw.ToString());
        }

        [Fact]
        public void UpdateDevice_DigitalLight_On_UpdatesStateAndPublishes()
        {
            var mockClient = new Mock<IMqttClient>();
            MqttApplicationMessage? captured = null;
            mockClient.Setup(m => m.PublishAsync(It.IsAny<MqttApplicationMessage>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync((MqttClientPublishResult)null)
                .Callback<MqttApplicationMessage, System.Threading.CancellationToken>((msg, _) => captured = msg);

            var comelit = CreateComelitWithClient(mockClient);

            var device = new DigitalLight { ID = "light1", SubType = Enums.OBJECT_SUBTYPE.DIGITAL_LIGHT };
            var method = typeof(MQTTComelit).GetMethod("UpdateDevice", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(comelit, new object[] { device, "ON" });

            Assert.Equal("1", device.Status);
            string lastCommand = (string)typeof(MQTTComelit).GetField("LastCommand", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(comelit)!;
            Assert.Contains(device.ID, lastCommand);
            Assert.Contains("token", lastCommand);
            Assert.NotNull(captured);
            Assert.Equal("topic", captured.Topic);
        }

        [Theory]
        [InlineData("heat", "\"act_type\":4", "\"act_params\":[1]", "0", "1")]
        [InlineData("cool", "\"act_type\":4", "\"act_params\":[0]", "1", "1")]
        [InlineData("off", "\"act_type\":0", "\"act_params\":[0]", "1", "1")]
        public void UpdateDeviceClimateMode_ValidMode_PublishesComelitPayload(string mode, string expectedActType, string expectedParams, string initialSeason, string initialPower)
        {
            var mockClient = new Mock<IMqttClient>();
            MqttApplicationMessage? captured = null;
            mockClient.Setup(m => m.PublishAsync(It.IsAny<MqttApplicationMessage>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync((MqttClientPublishResult)null)
                .Callback<MqttApplicationMessage, System.Threading.CancellationToken>((msg, _) => captured = msg);

            var comelit = CreateComelitWithClient(mockClient);
            var device = new Clima
            {
                ID = "clima1",
                SubType = Enums.OBJECT_SUBTYPE.CLIMA_THERMOSTAT_DEHUMIDIFIER,
                EstInv = initialSeason,
                PowerSt = initialPower,
                AutoMan = "2"
            };

            var method = typeof(MQTTComelit).GetMethod("UpdateDeviceClimateMode", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(comelit, new object[] { device, mode });

            Assert.NotNull(captured);
            string payload = (string)typeof(MQTTComelit).GetField("LastCommand", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(comelit)!;
            Assert.Contains(expectedActType, payload);
            Assert.Contains(expectedParams, payload);
            Assert.Contains("\"obj_id\":\"clima1\"", payload);
            Assert.Contains("\"sessiontoken\":\"token\"", payload);
        }

        [Theory]
        [InlineData("auto", "\"act_type\":13", "\"act_params\":[1]", "1")]
        [InlineData("manual", "\"act_type\":13", "\"act_params\":[2]", "2")]
        public void UpdateDeviceThermoControlMode_ValidMode_PublishesComelitPayload(string mode, string expectedActType, string expectedParams, string expectedAutoMan)
        {
            var mockClient = new Mock<IMqttClient>();
            mockClient.Setup(m => m.PublishAsync(It.IsAny<MqttApplicationMessage>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync((MqttClientPublishResult)null);

            var comelit = CreateComelitWithClient(mockClient);
            var device = new Clima
            {
                ID = "clima1",
                SubType = Enums.OBJECT_SUBTYPE.CLIMA_THERMOSTAT_DEHUMIDIFIER,
                PowerSt = "1",
                AutoMan = mode == "auto" ? "2" : "1"
            };

            var method = typeof(MQTTComelit).GetMethod("UpdateDeviceThermoControlMode", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(comelit, new object[] { device, mode });

            string payload = (string)typeof(MQTTComelit).GetField("LastCommand", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(comelit)!;
            Assert.Contains(expectedActType, payload);
            Assert.Contains(expectedParams, payload);
            Assert.Equal(expectedAutoMan, device.AutoMan);
        }

        [Theory]
        [InlineData("off", "\"act_type\":0", "\"act_params\":[2]", "5")]
        [InlineData("auto", "\"act_type\":23", "\"act_params\":[1]", "1")]
        [InlineData("manual", "\"act_type\":23", "\"act_params\":[2]", "2")]
        public void UpdateDeviceHumidityMode_ValidMode_PublishesComelitPayload(string mode, string expectedActType, string expectedParams, string expectedAutoManUmi)
        {
            var mockClient = new Mock<IMqttClient>();
            mockClient.Setup(m => m.PublishAsync(It.IsAny<MqttApplicationMessage>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync((MqttClientPublishResult)null);

            var comelit = CreateComelitWithClient(mockClient);
            var device = new Clima
            {
                ID = "clima1",
                SubType = Enums.OBJECT_SUBTYPE.CLIMA_THERMOSTAT_DEHUMIDIFIER,
                AutoManUmi = mode == "off" ? "2" : "5"
            };

            var method = typeof(MQTTComelit).GetMethod("UpdateDeviceHumidityMode", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(comelit, new object[] { device, mode });

            string payload = (string)typeof(MQTTComelit).GetField("LastCommand", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(comelit)!;
            Assert.Contains(expectedActType, payload);
            Assert.Contains(expectedParams, payload);
            Assert.Equal(expectedAutoManUmi, device.AutoManUmi);
        }

        [Fact]
        public void UpdateDeviceTemperature_ManualCooling_UpdatesActiveAndManualSetpoint()
        {
            var mockClient = new Mock<IMqttClient>();
            mockClient.Setup(m => m.PublishAsync(It.IsAny<MqttApplicationMessage>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync((MqttClientPublishResult)null);

            var comelit = CreateComelitWithClient(mockClient);
            var device = new Clima
            {
                ID = "clima1",
                SubType = Enums.OBJECT_SUBTYPE.CLIMA_THERMOSTAT_DEHUMIDIFIER,
                AutoMan = "2",
                EstInv = "0",
                SogliaAttiva = "280"
            };

            var method = typeof(MQTTComelit).GetMethod("UpdateDeviceTemperature", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(comelit, new object[] { device, "27.0" });

            string payload = (string)typeof(MQTTComelit).GetField("LastCommand", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(comelit)!;
            Assert.Contains("\"act_type\":2", payload);
            Assert.Contains("\"act_params\":[270]", payload);
            Assert.Equal("270", device.SogliaAttiva);
            Assert.Equal("270", device.SogliaManEst);
        }

        [Fact]
        public void UpdateDeviceHumidity_ManualDehumidifier_UpdatesActiveAndManualSetpoint()
        {
            var mockClient = new Mock<IMqttClient>();
            mockClient.Setup(m => m.PublishAsync(It.IsAny<MqttApplicationMessage>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync((MqttClientPublishResult)null);

            var comelit = CreateComelitWithClient(mockClient);
            var device = new Clima
            {
                ID = "clima1",
                SubType = Enums.OBJECT_SUBTYPE.CLIMA_THERMOSTAT_DEHUMIDIFIER,
                AutoManUmi = "2",
                SogliaAttivaUmi = "70"
            };

            var method = typeof(MQTTComelit).GetMethod("UpdateDeviceHumidity", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(comelit, new object[] { device, "65" });

            string payload = (string)typeof(MQTTComelit).GetField("LastCommand", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(comelit)!;
            Assert.Contains("\"act_type\":19", payload);
            Assert.Contains("\"act_params\":[65]", payload);
            Assert.Equal("65", device.SogliaAttivaUmi);
            Assert.Equal("65", device.SogliaManDeumi);
        }
    }
}
