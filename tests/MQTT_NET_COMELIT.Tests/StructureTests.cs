using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using MQTT_NET_COMELIT.Comelit;
using Xunit;
using static MQTT_NET_COMELIT.Comelit.JsonParsing;

namespace MQTT_NET_COMELIT.Tests
{
    public class StructureTests
    {
        [Fact]
        public void CreateStructure_TopLevelRuleWithoutArea_AddsRuleToSyntheticArea()
        {
            var comelit = (MQTTComelit)FormatterServices.GetUninitializedObject(typeof(MQTTComelit));
            var root = new Header
            {
                OutData =
                [
                    new OutData
                    {
                        Elements =
                        [
                            new Element
                            {
                                Data = new ElementData
                                {
                                    ID = "HOME#1",
                                    Description = "Casa",
                                    Type = Enums.OBJECT_TYPE.ZONE,
                                    SubType = Enums.OBJECT_SUBTYPE.GENERIC_ZONE,
                                    Elements = []
                                }
                            },
                            new Element
                            {
                                Data = new ElementData
                                {
                                    ID = "GEN#RL#36",
                                    Description = "Termostati con fotovoltaico",
                                    Type = Enums.OBJECT_TYPE.RULE,
                                    SubType = Enums.OBJECT_SUBTYPE.GENERIC,
                                    Status = "1"
                                }
                            }
                        ]
                    }
                ]
            };

            typeof(MQTTComelit)
                .GetMethod("CreateStructure", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(comelit, [root]);

            Area area = Assert.Single(comelit.HomeStructure.Areas);
            Device rule = Assert.Single(area.Devices);

            Assert.Equal("Senza area", area.Description);
            Assert.Equal("Termostati con fotovoltaico", rule.Description);
            Assert.Equal(Enums.OBJECT_TYPE.RULE, rule.Type);
            Assert.Equal("Senza area", rule.AreaName);
            Assert.Equal("home/rules/gen_rl_36/state", rule.StatusTopic);
            Assert.Equal("homeassistant/binary_sensor/gen_rl_36/config", rule.ConfigTopic);
            Assert.True(rule.ConfigReadyToSend);
            Assert.True(rule.CanSubscribe);
        }
    }
}
