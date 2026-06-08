using MQTT_NET_COMELIT.Comelit;
using MQTT_NET_COMELIT.HomeAssistant;
using Newtonsoft.Json;
using static MQTT_NET_COMELIT.Static.ConfigData;
using static MQTT_NET_COMELIT.Utility.Utility;

internal class Program
{
    static MQTTComelit MQTTComelit;
    static MQTTHomeAssistant MQTTHomeAssistant;
    static readonly string ConfigFile = Path.Combine(Environment.CurrentDirectory, "data", "options.json");
    static Config Config;

    private static async Task Main()
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
                SetLogLevel(Config.LogLevel);
                WriteLog("Initializing AddOn version 1.1.5");
                MQTTComelit = new MQTTComelit(Config.ComelitUsername, Config.ComelitPassword, Config.ComelitHUBMAC, Config.ComelitHUBIP, Config.ComelitHUBROOTElement, Config.PollingTime);
                MQTTHomeAssistant = new MQTTHomeAssistant(Config.HomeAssistantUsername, Config.HomeAssistantPassword, Config.HomeAssistantIP, MQTTComelit);
                MQTTComelit.MQTTHomeAssistant = MQTTHomeAssistant;

                WriteLog("Starting HA MQTT");
                MQTTHomeAssistant.Start();
                int homeAssistantWaitSeconds = 0;
                while (!MQTTHomeAssistant.ConnectedAndLoggedIn)
                {
                    if (homeAssistantWaitSeconds % 10 == 0) WriteLog("Waiting for HA...");
                    homeAssistantWaitSeconds++;
                    await Task.Delay(1000);
                }

                WriteLog("Starting Comelit MQTT");
                MQTTComelit.Start();
                int comelitWaitSeconds = 0;
                while (!MQTTComelit.ConnectedAndLoggedIn)
                {
                    if (comelitWaitSeconds % 10 == 0) WriteLog("Waiting for Comelit...");
                    comelitWaitSeconds++;
                    await Task.Delay(1000);
                }

                MQTTHomeAssistant.Publish(MQTTComelit.PollingStatus, "ON", true);

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
                                await Task.Delay(50);
                            }
                        }
                    }
                    MQTTHomeAssistant.Publish(MQTTComelit.PollingConfigTopic, MQTTComelit.PollingConfigPayload, true);
                    Config.InitializeDevicesConfiguration = false;
                    File.WriteAllText(ConfigFile, JsonConvert.SerializeObject(Config));
                }

                // Publish initial state for all known devices to Home Assistant
                foreach (Area area in MQTTComelit.HomeStructure.Areas)
                {
                    foreach (Device dev in area.Devices)
                    {
                        if (dev == null) continue;

                        if (dev is ComelitSensor sensor)
                        {
                            MQTTHomeAssistant.Publish(sensor.StatusTopic, sensor.SensorValue);
                            await Task.Delay(25);
                            continue;
                        }

                        // Device-specific initial state publishes
                        switch (dev.SubType)
                        {
                            case Enums.OBJECT_SUBTYPE.ELECTRIC_BLIND:
                            case Enums.OBJECT_SUBTYPE.ENHANCED_ELECTRIC_BLIND:
                                MQTTHomeAssistant.Publish(dev.StatusTopic, dev.CoverState);
                                await Task.Delay(25);
                                break;
                            default:
                                // Publish generic ON/OFF status if available
                                if (!string.IsNullOrEmpty(dev.StatusTopic))
                                {
                                    MQTTHomeAssistant.Publish(dev.StatusTopic, dev.StatusONOFF);
                                    await Task.Delay(25);
                                }
                                break;
                        }

                        switch (dev.SubType)
                        {
                            case Enums.OBJECT_SUBTYPE.DIMMER_LIGHT:
                                if (dev is DimmerLight dimmer)
                                {
                                    string bright = !string.IsNullOrEmpty(dimmer.Bright) ? dimmer.Bright : "0";
                                    MQTTHomeAssistant.Publish($"home/lights/{dev.GetIDForTopic()}/brightness/state", bright);
                                    await Task.Delay(25);
                                }
                                break;
                            case Enums.OBJECT_SUBTYPE.ELECTRIC_BLIND:
                            case Enums.OBJECT_SUBTYPE.ENHANCED_ELECTRIC_BLIND:
                                if (dev is ElectricBlind blind)
                                {
                                    string pos = !string.IsNullOrEmpty(blind.OpenStatus) ? blind.OpenStatus : "0";
                                    MQTTHomeAssistant.Publish($"home/cover/{dev.GetIDForTopic()}/position/state", pos);
                                    await Task.Delay(25);
                                }
                                break;
                            case Enums.OBJECT_SUBTYPE.CLIMA_THERMOSTAT_DEHUMIDIFIER:
                                if (dev is Clima clima)
                                {
                                    if (!string.IsNullOrEmpty(clima.Temperatura))
                                    {
                                        MQTTHomeAssistant.Publish($"home/climate/{dev.GetIDForTopic()}/current-temperature/state", NormalizeComelitTemperature(clima.Temperatura));
                                        await Task.Delay(25);
                                    }
                                    if (!string.IsNullOrEmpty(clima.Umidita))
                                    {
                                        MQTTHomeAssistant.Publish($"home/climate/{dev.GetIDForTopic()}/current-humidity/state", clima.Umidita);
                                        await Task.Delay(25);
                                    }
                                    MQTTComelit.PublishClimateState(clima);
                                    await Task.Delay(25);
                                }
                                break;
                        }
                    }
                }

                if (MQTTComelit.PollingDevice != null) MQTTComelit.StartPolling();
                WriteLog("Ready");
                bool lastReadyState = true;
                while (true)
                {
                    bool readyState = MQTTComelit.ConnectedAndLoggedIn && MQTTHomeAssistant.ConnectedAndLoggedIn;
                    if (readyState != lastReadyState)
                    {
                        WriteLog(readyState ? "Connection restored" : "Connection lost, waiting for reconnect...");
                        lastReadyState = readyState;
                    }

                    await Task.Delay(1000);
                }
            }
        }
        else
        {
            Console.WriteLine("Unable to initialize AddOn - Config file doesn't exist!");
        }
        Console.ReadLine();

    }
}
