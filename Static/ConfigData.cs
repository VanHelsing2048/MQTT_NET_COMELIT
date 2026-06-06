namespace MQTT_NET_COMELIT.Static
{
    public static class ConfigData
    {
        public class Config
        {
            [Newtonsoft.Json.JsonProperty("comelit-username")]
            public string ComelitUsername { get; set; }
            [Newtonsoft.Json.JsonProperty("comelit-password")]
            public string ComelitPassword { get; set; }
            [Newtonsoft.Json.JsonProperty("comelit-HubIPAddress")]
            public string ComelitHUBIP { get; set; }
            [Newtonsoft.Json.JsonProperty("comelit-HubMACAddress")]
            public string ComelitHUBMAC { get; set; }
            [Newtonsoft.Json.JsonProperty("comelit-ROOTelement")]
            public string ComelitHUBROOTElement { get; set; }
            [Newtonsoft.Json.JsonProperty("polling")]
            public int PollingTime { get; set; }
            [Newtonsoft.Json.JsonProperty("mosquitto-username")]
            public string HomeAssistantUsername { get; set; }
            [Newtonsoft.Json.JsonProperty("mosquitto-password")]
            public string HomeAssistantPassword { get; set; }
            [Newtonsoft.Json.JsonProperty("mosquitto-IPAddress")]
            public string HomeAssistantIP { get; set; }
            [Newtonsoft.Json.JsonProperty("init-device-config")]
            public bool InitializeDevicesConfiguration { get; set; }
            [Newtonsoft.Json.JsonProperty("log-level")]
            public string LogLevel { get; set; } = "debug";
        }
    }
}
