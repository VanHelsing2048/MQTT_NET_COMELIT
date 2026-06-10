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
    }
}
