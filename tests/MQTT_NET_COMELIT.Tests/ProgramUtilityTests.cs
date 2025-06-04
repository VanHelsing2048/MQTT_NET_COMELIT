using MQTT_NET_COMELIT.Utility;
using System;
using System.IO;
using Xunit;

namespace MQTT_NET_COMELIT.Tests
{
    public class ProgramUtilityTests
    {
        [Fact]
        public void WriteLog_Writes_Message_WithTimestamp()
        {
            var sw = new StringWriter();
            Console.SetOut(sw);

            Utility.WriteLog("hello");

            var output = sw.ToString().Trim();
            Assert.EndsWith(" - hello", output);
        }
    }
}
