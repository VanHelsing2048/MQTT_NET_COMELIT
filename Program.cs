using MQTT_NET_COMELIT.Comelit;
using MQTT_NET_COMELIT.HomeAssistant;
using Newtonsoft.Json;
using static MQTT_NET_COMELIT.Static.ConfigData;
using static MQTT_NET_COMELIT.Utility.Utility;
using MQTT_NET_COMELIT.Comelit.DevicesStructure;

internal class Program
{
    static MQTTComelit MQTTComelit;
    static MQTTHomeAssistant MQTTHomeAssistant;
    static readonly string ConfigFile = Path.Combine(Environment.CurrentDirectory, "data", "options.json");
    static Config Config;

    private static void Main()
    {
        if (File.Exists(ConfigFile))
        {
            Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigFile));
            if (Config == null)
            {
                Console.WriteLine("Unable to initialize AddOn - Config is broken!");
            }
            else
            {
                WriteLog("Starting Comelit MQTT");
                MQTTComelit = new MQTTComelit(Config.ComelitUsername, Config.ComelitPassword, Config.ComelitHUBMAC, Config.ComelitHUBIP, Config.ComelitHUBROOTElement, Config.PollingTime);
                while (!MQTTComelit.ConnectedAndLoggedIn) { WriteLog("Waiting for Comelit..."); Task.Delay(1000).Wait(); }

                WriteLog("Starting HA MQTT");
                MQTTHomeAssistant = new MQTTHomeAssistant(Config.HomeAssistantUsername, Config.HomeAssistantPassword, Config.HomeAssistantIP, MQTTComelit);
                while (!MQTTHomeAssistant.ConnectedAndLoggedIn) { WriteLog("Waiting for HA..."); Task.Delay(1000).Wait(); }

                MQTTComelit.MQTTHomeAssistant = MQTTHomeAssistant;

                //Invio configurazione luci ad HA per MQTT Discovery + aggiornamento immediato dello stato
                if (Config.InitializeDevicesConfiguration)
                {
                    foreach (Area area in MQTTComelit.HomeStructure.Areas)
                    {
                        foreach (Device dev in area.Devices)
                        {
                            if (dev != null && dev.ConfigReadyToSend)
                            {
                                MQTTHomeAssistant.Publish(dev.ConfigTopic, dev.ConfigPayload, true);
                                Task.Delay(50).Wait();
                            }
                        }
                    }
                    MQTTHomeAssistant.Publish(MQTTComelit.PollingConfigTopic, MQTTComelit.PollingConfigPayload, true);
                    Config.InitializeDevicesConfiguration = false;
                    File.WriteAllText(ConfigFile, JsonConvert.SerializeObject(Config));
                }

                foreach (Area area in MQTTComelit.HomeStructure.Areas)
                {
                    foreach (Device dev in area.Devices)
                    {
                        if (dev?.SubType == Enums.OBJECT_SUBTYPE.DIGITAL_LIGHT || 
                            dev?.Type == Enums.OBJECT_TYPE.IRRIGATION || 
                            dev?.Type == Enums.OBJECT_TYPE.INPUT ||
                            dev?.SubType == Enums.OBJECT_SUBTYPE.OTHER_DIGIT || 
                            dev?.Type == Enums.OBJECT_TYPE.RULE)
                        {
                            MQTTHomeAssistant.Publish(dev.StatusTopic, $"{dev.StatusONOFF}");
                            Task.Delay(50).Wait();
                        }
                    }
                }
                if (MQTTComelit.PollingDevice != null) MQTTComelit.StartPolling();
                WriteLog("Ready");
                while (MQTTComelit.ConnectedAndLoggedIn && MQTTHomeAssistant.ConnectedAndLoggedIn) { Task.Delay(1000).Wait(); }
            }
        }
        else
        {
            Console.WriteLine("Unable to initialize AddOn - Config file doesn't exist!");
        }
        Console.ReadLine();

    }
}
