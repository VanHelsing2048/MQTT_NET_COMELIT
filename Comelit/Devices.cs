using static MQTT_NET_COMELIT.Comelit.Enums;

namespace MQTT_NET_COMELIT.Comelit
{
    public static class Devices
    {
        public static void CreateDigitalLightConfig(DigitalLight dev)
        {
            dev.CommandTopic = $"home/lights/{dev.GetIDForTopic()}/set";
            dev.StatusTopic = $"home/lights/{dev.GetIDForTopic()}/state";
            dev.ConfigPayload = new MQTTConfig() { 
                Name = dev.Description, 
                ObjectId = dev.GetIDForTopic(), 
                UniqueId = dev.GetIDForTopic(), 
                Icon = "mdi:lightbulb", 
                CommandTopic = dev.CommandTopic, 
                StateTopic = dev.StatusTopic,
                PayloadOn = "ON",
                PayloadOff = "OFF",
                Device = new MQTTDevice() { Identifiers = [ dev.AreaName ], Manufacturer = "Comelit", Model = "Comelit", Name = "Comelit Light", SuggestedArea = dev.AreaName } 
            }.ToString();
            dev.ConfigTopic = $"homeassistant/light/{dev.GetIDForTopic()}/config";
            dev.ConfigReadyToSend = true;
            dev.CanSubscribe = true;
        }
        public static void CreateDimmerLightConfig(DimmerLight dev)
        {
            dev.CommandTopic = $"home/lights/{dev.GetIDForTopic()}/set";
            dev.StatusTopic = $"home/lights/{dev.GetIDForTopic()}/state";
            dev.ConfigPayload = new MQTTConfig() { 
                Name = dev.Description, 
                ObjectId = dev.GetIDForTopic(), 
                UniqueId = dev.GetIDForTopic(), 
                Icon = "mdi:lightbulb", 
                CommandTopic = dev.CommandTopic, 
                StateTopic = dev.StatusTopic,
                PayloadOn = "ON",
                PayloadOff = "OFF",
                BrightnessCommandTopic = $"home/lights/{dev.GetIDForTopic()}/brightness/set",
                BrightnessStateTopic = $"home/lights/{dev.GetIDForTopic()}/brightness/state",
                BrightnessScale = "255",
                Device = new MQTTDevice() { Identifiers = [dev.AreaName], Manufacturer = "Comelit", Model = "Comelit", Name = "Comelit Dimmer Light", SuggestedArea = dev.AreaName } 
            }.ToString();
            dev.ConfigTopic = $"homeassistant/light/{dev.GetIDForTopic()}/config";
            dev.ConfigReadyToSend = true;
            dev.CanSubscribe = true;
        }
        public static void CreateElectricBlindConfig(ElectricBlind dev)
        {
            dev.CommandTopic = $"home/cover/{dev.GetIDForTopic()}/set";
            dev.StatusTopic = $"home/cover/{dev.GetIDForTopic()}/state";
            dev.ConfigPayload = new MQTTConfig() {
                Name = dev.Description, 
                DeviceClass = "cover", 
                ObjectId = dev.GetIDForTopic(), 
                UniqueId = dev.GetIDForTopic(), 
                Icon = "mdi:blinds", 
                CommandTopic = dev.CommandTopic, 
                StateTopic = dev.StatusTopic,
                PayloadOpen = "OPEN",
                PayloadClose = "CLOSE",
                PayloadStop = "STOP",
                PositionTopic = $"home/cover/{dev.GetIDForTopic()}/position/state",
                PositionOpenValue = "100",
                PositionClosedValue = "0",
                Device = new MQTTDevice() { Identifiers = [dev.AreaName], Manufacturer = "Comelit", Model = "Comelit", Name = "Comelit Electric Blind", SuggestedArea = dev.AreaName } 
            }.ToString();
            dev.ConfigTopic = $"homeassistant/cover/{dev.GetIDForTopic()}/config";
            dev.ConfigReadyToSend = true;
            dev.CanSubscribe = true;
        }
        public static void CreateOtherDigitConfig(OtherDigit dev)
        {
            dev.CommandTopic = $"home/switch/{dev.GetIDForTopic()}/set";
            dev.StatusTopic = $"home/switch/{dev.GetIDForTopic()}/state";
            dev.ConfigPayload = new MQTTConfig() { 
                Name = dev.Description, 
                DeviceClass = "switch", 
                ObjectId = dev.GetIDForTopic(), 
                UniqueId = dev.GetIDForTopic(), 
                Icon = "mdi:switch", 
                CommandTopic = dev.CommandTopic, 
                StateTopic = dev.StatusTopic,
                PayloadOn = "ON",
                PayloadOff = "OFF",
                Device = new MQTTDevice() { Identifiers = [dev.AreaName], Manufacturer = "Comelit", Model = "Comelit", Name = "Comelit Digital Output", SuggestedArea = dev.AreaName } 
            }.ToString();
            dev.ConfigTopic = $"homeassistant/switch/{dev.GetIDForTopic()}/config";
            dev.ConfigReadyToSend = true;
            dev.CanSubscribe = true;
        }
        public static void CreateOtherTemporaryConfig(OtherTemporary dev)
        {
            dev.CommandTopic = $"home/buttons/{dev.GetIDForTopic()}/press";
            dev.ConfigPayload = new MQTTConfig() {
                Name = dev.Description,
                ObjectId = dev.GetIDForTopic(),
                UniqueId = dev.GetIDForTopic(),
                Icon = "mdi:gesture-tap-button",
                CommandTopic = dev.CommandTopic,
                PayloadPress = "PRESS",
                Device = new MQTTDevice() { Identifiers = [dev.AreaName], Manufacturer = "Comelit", Model = "Comelit", Name = "Comelit Temporary Output", SuggestedArea = dev.AreaName }
            }.ToString();
            dev.ConfigTopic = $"homeassistant/button/{dev.GetIDForTopic()}/config";
            dev.ConfigReadyToSend = true;
            dev.CanSubscribe = true;
        }
        public static void CreateAutomationConfig(Automation dev)
        {
            dev.CommandTopic = $"home/buttons/{dev.GetIDForTopic()}/press";
            dev.ConfigPayload = new MQTTConfig() {
                Name = dev.Description,
                ObjectId = dev.GetIDForTopic(),
                UniqueId = dev.GetIDForTopic(),
                Icon = "mdi:window-open-variant",
                CommandTopic = dev.CommandTopic,
                PayloadPress = "PRESS",
                Device = new MQTTDevice() { Identifiers = [dev.AreaName], Manufacturer = "Comelit", Model = "Comelit", Name = "Comelit Automation", SuggestedArea = dev.AreaName }
            }.ToString();
            dev.ConfigTopic = $"homeassistant/button/{dev.GetIDForTopic()}/config";
            dev.ConfigReadyToSend = true;
            dev.CanSubscribe = true;
        }
        public static void CreateSensorConfig(ComelitSensor dev)
        {
            dev.StatusTopic = $"home/sensor/{dev.GetIDForTopic()}/state";
            dev.ConfigPayload = new MQTTConfig() {
                Name = dev.Description,
                ObjectId = dev.GetIDForTopic(),
                UniqueId = dev.GetIDForTopic(),
                DeviceClass = dev.DeviceClass,
                Icon = dev.Icon,
                StateTopic = dev.StatusTopic,
                UnitOfMeasurement = dev.UnitOfMeasurement,
                StateClass = dev.StateClass,
                Device = new MQTTDevice() { Identifiers = [dev.AreaName], Manufacturer = "Comelit", Model = "Comelit", Name = "Comelit Sensor", SuggestedArea = dev.AreaName }
            }.ToString();
            dev.ConfigTopic = $"homeassistant/sensor/{dev.GetIDForTopic()}/config";
            dev.ConfigReadyToSend = true;
            dev.CanSubscribe = true;
        }
        public static void CreateClimaConfig(Clima dev)
        {
            dev.CommandTopic = $"home/climate/{dev.GetIDForTopic()}/set";
            dev.StatusTopic = $"home/climate/{dev.GetIDForTopic()}/state";
            dev.ConfigPayload = new MQTTConfig() { 
                Name = dev.Description, 
                DeviceClass = "climate", 
                ObjectId = dev.GetIDForTopic(), 
                UniqueId = dev.GetIDForTopic(), 
                Icon = "mdi:thermostat", 
                CommandTopic = dev.CommandTopic, 
                StateTopic = dev.StatusTopic,
                CurrentTemperatureTopic = $"home/climate/{dev.GetIDForTopic()}/current-temperature/state",
                TemperatureStateTopic = $"home/climate/{dev.GetIDForTopic()}/target-temperature/state",
                TemperatureCommandTopic = $"home/climate/{dev.GetIDForTopic()}/target-temperature/set",
                ModeStateTopic = $"home/climate/{dev.GetIDForTopic()}/mode/state",
                ActionTopic = $"home/climate/{dev.GetIDForTopic()}/action/state",
                PresetModeStateTopic = $"home/climate/{dev.GetIDForTopic()}/preset-mode/state",
                MinTemp = "15",
                MaxTemp = "30",
                TempStep = "0.5",
                Modes = new List<string> { "off", "heat", "cool" },
                PresetModes = new List<string> { "auto", "manual", "semiauto", "off" },
                CurrentHumidityTopic = $"home/climate/{dev.GetIDForTopic()}/current-humidity/state",
                TargetHumidityStateTopic = $"home/climate/{dev.GetIDForTopic()}/target-humidity/state",
                TargetHumidityCommandTopic = $"home/climate/{dev.GetIDForTopic()}/target-humidity/set",
                Device = new MQTTDevice() { Identifiers = [dev.AreaName], Manufacturer = "Comelit", Model = "Comelit", Name = "Comelit Climate", SuggestedArea = dev.AreaName } 
            }.ToString();
            dev.ConfigTopic = $"homeassistant/climate/{dev.GetIDForTopic()}/config";
            dev.ConfigReadyToSend = true;
            dev.CanSubscribe = true;
        }
        public static void CreateIrrigationValveConfig(IrrigationValve dev)
        {
            //configurazione come switch mdi:pipe-valve
            dev.CommandTopic = $"home/valves/{dev.GetIDForTopic()}/set";
            dev.StatusTopic = $"home/valves/{dev.GetIDForTopic()}/state";
            dev.ConfigPayload = new MQTTConfig() { 
                Name = dev.Description, 
                DeviceClass = "switch", 
                ObjectId = dev.GetIDForTopic(), 
                UniqueId = dev.GetIDForTopic(), 
                Icon = "mdi:pipe-valve", 
                CommandTopic = dev.CommandTopic, 
                StateTopic = dev.StatusTopic,
                PayloadOn = "ON",
                PayloadOff = "OFF",
                Device = new MQTTDevice() { Identifiers = [dev.AreaName], Manufacturer = "Comelit", Model = "Comelit", Name = "Comelit Valve", SuggestedArea = dev.AreaName } 
            }.ToString();
            dev.ConfigTopic = $"homeassistant/switch/{dev.GetIDForTopic()}/config";
            dev.ConfigReadyToSend = true;
            dev.CanSubscribe = true;
        }

        public static void CreateInputConfig(Input dev)
        {
            dev.StatusTopic = $"home/inputs/{dev.GetIDForTopic()}/state";
            dev.ConfigPayload = new MQTTConfig() { 
                Name = dev.Description, 
                ObjectId = dev.GetIDForTopic(), 
                UniqueId = dev.GetIDForTopic(), 
                StateTopic = dev.StatusTopic,
                PayloadOn = "ON",
                PayloadOff = "OFF",
                Device = new MQTTDevice() { Identifiers = [dev.AreaName], Manufacturer = "Comelit", Model = "Comelit", Name = "Comelit Input", SuggestedArea = dev.AreaName } 
            }.ToString();
            dev.ConfigTopic = $"homeassistant/binary_sensor/{dev.GetIDForTopic()}/config";
            dev.ConfigReadyToSend = true;
            dev.CanSubscribe = true;
        }

        internal static void CreateAlarmConfig(Input dev)
        {
            dev.StatusTopic = $"home/inputs/{dev.GetIDForTopic()}/state";
            dev.ConfigPayload = new MQTTConfig() { 
                Name = dev.Description, 
                ObjectId = dev.GetIDForTopic(), 
                UniqueId = dev.GetIDForTopic(), 
                DeviceClass = GetAlarmDeviceClass(dev.Description),
                StateTopic = dev.StatusTopic,
                PayloadOn = "ON",
                PayloadOff = "OFF",
                Device = new MQTTDevice() { Identifiers = [dev.AreaName], Manufacturer = "Comelit", Model = "Comelit", Name = "Comelit Alarm", SuggestedArea = dev.AreaName } 
            }.ToString();
            dev.ConfigTopic = $"homeassistant/binary_sensor/{dev.GetIDForTopic()}/config";
            dev.ConfigReadyToSend = true;
            dev.CanSubscribe = true;
        }

        private static string GetAlarmDeviceClass(string description)
        {
            description ??= string.Empty;

            if (description.StartsWith("IR ", StringComparison.OrdinalIgnoreCase)
                || description.StartsWith("BR ", StringComparison.OrdinalIgnoreCase))
            {
                return "motion";
            }

            if (description.StartsWith("CT ", StringComparison.OrdinalIgnoreCase))
            {
                if (description.Contains("porta", StringComparison.OrdinalIgnoreCase)
                    || description.Contains("scorrevole", StringComparison.OrdinalIgnoreCase)
                    || description.Contains("basculant", StringComparison.OrdinalIgnoreCase))
                {
                    return "door";
                }

                if (description.Contains("finestr", StringComparison.OrdinalIgnoreCase))
                {
                    return "window";
                }

                return "opening";
            }

            return "safety";
        }
    }
}

namespace MQTT_NET_COMELIT.Comelit
{
    public abstract class Device
    {
        [Newtonsoft.Json.JsonIgnore]
        public string ConfigPayload { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string ConfigTopic { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string AreaName { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string CommandTopic { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string StatusTopic { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string StatusONOFF => Status != "0" ? "ON" : "OFF"; //Da discriminare meglio per gli input - Potrebbero arrivare dei 255 invece di 1
        [Newtonsoft.Json.JsonIgnore]
        public bool ConfigReadyToSend { get; set; } = false;
        [Newtonsoft.Json.JsonIgnore]
        public bool CanSubscribe { get; set; } = false;
        [Newtonsoft.Json.JsonIgnore]
        public bool IsPollingDevice { get; set; } = false;
        [Newtonsoft.Json.JsonProperty("param_type")]
        public string ParamType { get; set; }
        [Newtonsoft.Json.JsonProperty("obj_id")]
        public string ObjID { get; set; }
        [Newtonsoft.Json.JsonProperty("condition")]
        public string Condition { get; set; }

        [Newtonsoft.Json.JsonProperty("id")]
        public string ID { get; set; }
        [Newtonsoft.Json.JsonProperty("type")]
        public OBJECT_TYPE Type { get; set; }
        [Newtonsoft.Json.JsonProperty("sub_type")]
        public OBJECT_SUBTYPE SubType { get; set; }
        [Newtonsoft.Json.JsonProperty("descrizione")]
        public string Description { get; set; }
        [Newtonsoft.Json.JsonProperty("sched_status")]
        public string SchedStatus { get; set; }
        [Newtonsoft.Json.JsonProperty("sched_lock")]
        public string SchedLock { get; set; }
        [Newtonsoft.Json.JsonProperty("status")]
        public string Status { get; set; }
        [Newtonsoft.Json.JsonProperty("powerst")]
        public string PowerSt { get; set; }
        [Newtonsoft.Json.JsonProperty("num_modulo")]
        public string NumModulo { get; set; }
        [Newtonsoft.Json.JsonProperty("num_uscita")]
        public string NumUscita { get; set; }
        [Newtonsoft.Json.JsonProperty("icon_id")]
        public string IconID { get; set; }
        [Newtonsoft.Json.JsonProperty("isProtected")]
        public string IsProtected { get; set; }
        [Newtonsoft.Json.JsonProperty("objectId")]
        public string ObjectID { get; set; }
        [Newtonsoft.Json.JsonProperty("placeId")]
        public string PlaceID { get; set; }        
        [Newtonsoft.Json.JsonProperty("num_ingresso")]
        public string NumIngresso { get; set; }

        public string GetIDForTopic()
        {
            return MQTT_NET_COMELIT.Utility.Utility.NormalizeComelitId(ID);
        }

        /// <summary>
        /// Create a state snapshot for change detection.
        /// Can be overridden by subclasses if custom state tracking is needed.
        /// </summary>
        public virtual DeviceStateManager.DeviceStateSnapshot GetStateSnapshot()
        {
            return DeviceStateManager.DeviceStateSnapshot.FromDevice(this);
        }
    }

    public class DigitalLight : Device { }

    public class DimmerLight : Device
    {
        [Newtonsoft.Json.JsonProperty("zero_dimmer")]
        public string ZeroDimmer { get; set; }
        [Newtonsoft.Json.JsonProperty("presenza")]
        public string Presenza { get; set; }
        [Newtonsoft.Json.JsonProperty("value")]
        public string Value { get; set; }
        [Newtonsoft.Json.JsonProperty("bright")]
        public string Bright { get; set; }
        [Newtonsoft.Json.JsonProperty("presenza_on")]
        public string PresenzaON { get; set; }
        [Newtonsoft.Json.JsonProperty("presente")]
        public string Presente { get; set; }
    }

    public class ElectricBlind : Device
    {
        [Newtonsoft.Json.JsonProperty("openTime")]
        public string OpenTime { get; set; }
        [Newtonsoft.Json.JsonProperty("closeTime")]
        public string CloseTime { get; set; }
        [Newtonsoft.Json.JsonProperty("open_status")]
        public string OpenStatus { get; set; }
    }

    public class OtherDigit : Device { }

    public class OtherTemporary : Device { }

    public class Automation : Device { }

    public class ComelitSensor : Device
    {
        [Newtonsoft.Json.JsonProperty("instant_power")]
        public string InstantPower { get; set; }
        [Newtonsoft.Json.JsonProperty("label_value")]
        public string LabelValue { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string UnitOfMeasurement { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string DeviceClass { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string StateClass { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string Icon { get; set; }

        public string SensorValue => !string.IsNullOrWhiteSpace(InstantPower) ? InstantPower : Status;
    }

    public class Input : Device { }

    public class IrrigationValve : Device { }

    public class Clima : Device
    {
        [Newtonsoft.Json.JsonProperty("num_moduloIE")]
        public string NumModuloIE { get; set; }

        [Newtonsoft.Json.JsonProperty("num_uscitaIE")]
        public string NumUscitaIE { get; set; }

        [Newtonsoft.Json.JsonProperty("num_moduloI")]
        public string NumModuloI { get; set; }

        [Newtonsoft.Json.JsonProperty("num_uscitaI")]
        public string NumUscitaI { get; set; }

        [Newtonsoft.Json.JsonProperty("num_moduloE")]
        public string NumModuloE { get; set; }

        [Newtonsoft.Json.JsonProperty("num_uscitaE")]
        public string NumUscitaE { get; set; }

        [Newtonsoft.Json.JsonProperty("num_moduloI_ana")]
        public string NumModuloIAna { get; set; }

        [Newtonsoft.Json.JsonProperty("num_uscitaI_ana")]
        public string NumUscitaIAna { get; set; }

        [Newtonsoft.Json.JsonProperty("num_moduloE_ana")]
        public string NumModuloEAna { get; set; }

        [Newtonsoft.Json.JsonProperty("num_uscitaE_ana")]
        public string NumUscitaEAna { get; set; }

        [Newtonsoft.Json.JsonProperty("soglia_man_inv")]
        public string SogliaManInv { get; set; }

        [Newtonsoft.Json.JsonProperty("soglia_man_est")]
        public string SogliaManEst { get; set; }

        [Newtonsoft.Json.JsonProperty("soglia_man_notte_inv")]
        public string SogliaManNotteInv { get; set; }

        [Newtonsoft.Json.JsonProperty("soglia_man_notte_est")]
        public string SogliaManNotteEst { get; set; }

        [Newtonsoft.Json.JsonProperty("soglia_semiauto")]
        public string SogliaSemiauto { get; set; }

        [Newtonsoft.Json.JsonProperty("night_mode")]
        public string NightMode { get; set; }

        [Newtonsoft.Json.JsonProperty("soglia_auto_inv")]
        public string SogliaAutoInv { get; set; }

        [Newtonsoft.Json.JsonProperty("soglia_auto_est")]
        public string SogliaAutoEst { get; set; }

        [Newtonsoft.Json.JsonProperty("out_enable_inv")]
        public string OutEnableInv { get; set; }

        [Newtonsoft.Json.JsonProperty("out_enable_est")]
        public string OutEnableEst { get; set; }

        [Newtonsoft.Json.JsonProperty("dir_enable_inv")]
        public string DirEnableInv { get; set; }

        [Newtonsoft.Json.JsonProperty("dir_enable_est")]
        public string DirEnableEst { get; set; }

        [Newtonsoft.Json.JsonProperty("heatAutoFanDisable")]
        public string HeatAutoFanDisable { get; set; }

        [Newtonsoft.Json.JsonProperty("coolAutoFanDisable")]
        public string CoolAutoFanDisable { get; set; }

        [Newtonsoft.Json.JsonProperty("heatSwingDisable")]
        public string HeatSwingDisable { get; set; }

        [Newtonsoft.Json.JsonProperty("coolSwingDisable")]
        public string CoolSwingDisable { get; set; }

        [Newtonsoft.Json.JsonProperty("out_type_inv")]
        public string OutTypeInv { get; set; }

        [Newtonsoft.Json.JsonProperty("out_type_est")]
        public string OutTypeEst { get; set; }

        [Newtonsoft.Json.JsonProperty("temp_base_inv")]
        public string TempBaseInv { get; set; }

        [Newtonsoft.Json.JsonProperty("temp_base_est")]
        public string TempBaseEst { get; set; }

        [Newtonsoft.Json.JsonProperty("num_moduloUD")]
        public string NumModuloUD { get; set; }

        [Newtonsoft.Json.JsonProperty("num_uscitaUD")]
        public string NumUscitaUD { get; set; }

        [Newtonsoft.Json.JsonProperty("num_moduloU")]
        public string NumModuloU { get; set; }

        [Newtonsoft.Json.JsonProperty("num_uscitaU")]
        public string NumUscitaU { get; set; }

        [Newtonsoft.Json.JsonProperty("num_moduloD")]
        public string NumModuloD { get; set; }

        [Newtonsoft.Json.JsonProperty("num_uscitaD")]
        public string NumUscitaD { get; set; }

        [Newtonsoft.Json.JsonProperty("num_moduloU_ana")]
        public string NumModuloUAna { get; set; }

        [Newtonsoft.Json.JsonProperty("num_uscitaU_ana")]
        public string NumUscitaUAna { get; set; }

        [Newtonsoft.Json.JsonProperty("num_moduloD_ana")]
        public string NumModuloDAna { get; set; }

        [Newtonsoft.Json.JsonProperty("num_uscitaD_ana")]
        public string NumUscitaDAna { get; set; }

        [Newtonsoft.Json.JsonProperty("out_enable_umi")]
        public string OutEnableUmi { get; set; }

        [Newtonsoft.Json.JsonProperty("out_enable_deumi")]
        public string OutEnableDeumi { get; set; }

        [Newtonsoft.Json.JsonProperty("dir_enable_umi")]
        public string DirEnableUmi { get; set; }

        [Newtonsoft.Json.JsonProperty("dir_enable_deumi")]
        public string DirEnableDeumi { get; set; }

        [Newtonsoft.Json.JsonProperty("humAutoFanDisable")]
        public string HumAutoFanDisable { get; set; }

        [Newtonsoft.Json.JsonProperty("dehumAutoFanDisable")]
        public string DehumAutoFanDisable { get; set; }

        [Newtonsoft.Json.JsonProperty("humSwingDisable")]
        public string HumSwingDisable { get; set; }

        [Newtonsoft.Json.JsonProperty("dehumSwingDisable")]
        public string DehumSwingDisable { get; set; }

        [Newtonsoft.Json.JsonProperty("out_type_umi")]
        public string OutTypeUmi { get; set; }

        [Newtonsoft.Json.JsonProperty("out_type_deumi")]
        public string OutTypeDeumi { get; set; }

        [Newtonsoft.Json.JsonProperty("soglia_man_umi")]
        public string SogliaManUmi { get; set; }

        [Newtonsoft.Json.JsonProperty("soglia_man_deumi")]
        public string SogliaManDeumi { get; set; }

        [Newtonsoft.Json.JsonProperty("soglia_man_notte_umi")]
        public string SogliaManNotteUmi { get; set; }

        [Newtonsoft.Json.JsonProperty("soglia_man_notte_deumi")]
        public string SogliaManNotteDeumi { get; set; }

        [Newtonsoft.Json.JsonProperty("night_mode_umi")]
        public string NightModeUmi { get; set; }

        [Newtonsoft.Json.JsonProperty("soglia_semiauto_umi")]
        public string SogliaSemiautoUmi { get; set; }

        [Newtonsoft.Json.JsonProperty("umi_base_umi")]
        public string UmiBaseUmi { get; set; }

        [Newtonsoft.Json.JsonProperty("umi_base_deumi")]
        public string UmiBaseDeumi { get; set; }

        [Newtonsoft.Json.JsonProperty("coolLimitMax")]
        public string CoolLimitMax { get; set; }

        [Newtonsoft.Json.JsonProperty("coolLimitMin")]
        public string CoolLimitMin { get; set; }

        [Newtonsoft.Json.JsonProperty("heatLimitMax")]
        public string HeatLimitMax { get; set; }

        [Newtonsoft.Json.JsonProperty("heatLimitMin")]
        public string HeatLimitMin { get; set; }

        [Newtonsoft.Json.JsonProperty("humidityViewOnly")]
        public string HumidityViewOnly { get; set; }

        [Newtonsoft.Json.JsonProperty("thermoViewOnly")]
        public string ThermoViewOnly { get; set; }

        [Newtonsoft.Json.JsonProperty("viewOnly")]
        public string ViewOnly { get; set; }

        [Newtonsoft.Json.JsonProperty("temperatura")]
        public string Temperatura { get; set; }

        [Newtonsoft.Json.JsonProperty("auto_man")]
        public string AutoMan { get; set; }

        [Newtonsoft.Json.JsonProperty("est_inv")]
        public string EstInv { get; set; }

        [Newtonsoft.Json.JsonProperty("soglia_attiva")]
        public string SogliaAttiva { get; set; }

        [Newtonsoft.Json.JsonProperty("out_value_inv")]
        public string OutValueInv { get; set; }

        [Newtonsoft.Json.JsonProperty("out_value_est")]
        public string OutValueEst { get; set; }

        [Newtonsoft.Json.JsonProperty("dir_out_inv")]
        public string DirOutInv { get; set; }

        [Newtonsoft.Json.JsonProperty("dir_out_est")]
        public string DirOutEst { get; set; }

        [Newtonsoft.Json.JsonProperty("semiauto_enabled")]
        public string SemiautoEnabled { get; set; }

        [Newtonsoft.Json.JsonProperty("umidita")]
        public string Umidita { get; set; }

        [Newtonsoft.Json.JsonProperty("auto_man_umi")]
        public string AutoManUmi { get; set; }

        [Newtonsoft.Json.JsonProperty("deumi_umi")]
        public string DeumiUmi { get; set; }

        [Newtonsoft.Json.JsonProperty("soglia_attiva_umi")]
        public string SogliaAttivaUmi { get; set; }

        [Newtonsoft.Json.JsonProperty("semiauto_umi_enabled")]
        public string SemiautoUmiEnabled { get; set; }
    }

}
