using static MQTT_NET_COMELIT.Comelit.Enums;

namespace MQTT_NET_COMELIT.Comelit
{
    public static class Devices
    {
        public static void CreateDigitalLightConfig(DevicesStructure.DigitalLight dev)
        {
            dev.CommandTopic = $"home/lights/{dev.GetIDForTopic()}/set";
            dev.StatusTopic = $"home/lights/{dev.GetIDForTopic()}/state";
            dev.ConfigPayload = new MQTTConfig() { name = dev.Description, object_id = dev.ID, unique_id = dev.ID, icon = "mdi:lightbulb", command_topic = dev.CommandTopic, state_topic = dev.StatusTopic, device = new MQTTDevice() { identifiers = dev.AreaName, manufacturer = "Comelit", model = "Comelit", name = "Comelit Light", suggested_area = dev.AreaName } }.ToString();
            dev.ConfigTopic = $"homeassistant/light/{dev.GetIDForTopic()}/config";
            dev.ConfigReadyToSend = true;
            dev.CanSubscribe = true;
        }
        public static void CreateDimmerLightConfig(DevicesStructure.DimmerLight dev)
        {
            dev.CommandTopic = "";
            dev.StatusTopic = "";
            dev.ConfigPayload = "";
            dev.ConfigTopic = "";
        }
        public static void CreateElectricBlindConfig(DevicesStructure.ElectricBlind dev)
        {
            dev.CommandTopic = $"home/cover/{dev.GetIDForTopic()}/set";
            dev.StatusTopic = $"home/cover/{dev.GetIDForTopic()}/state";
            dev.ConfigPayload = new MQTTConfig() {name = dev.Description, device_class = "cover", object_id=dev.ID, unique_id = dev.ID, icon ="mdi:blinds", command_topic = dev.CommandTopic, state_topic = dev.StatusTopic, device = new MQTTDevice() { identifiers = dev.AreaName, manufacturer = "Comelit", model = "Comelit", name = "Comelit Electric Blind", suggested_area = dev.AreaName } }.ToString();
            dev.ConfigTopic = $"homeassistant/cover/{dev.GetIDForTopic()}/config";
            //dev.ConfigReadyToSend = true;
            //dev.CanSubscribe = true;
        }
        public static void CreateOtherDigitConfig(DevicesStructure.OtherDigit dev)
        {
            dev.CommandTopic = $"home/switch/{dev.GetIDForTopic()}/set";
            dev.StatusTopic = $"home/switch/{dev.GetIDForTopic()}/state";
            dev.ConfigPayload = new MQTTConfig() { name = dev.Description, device_class = "switch", object_id = dev.ID, unique_id = dev.ID, icon = "mdi:switch", command_topic = dev.CommandTopic, state_topic = dev.StatusTopic, device = new MQTTDevice() { identifiers = dev.AreaName, manufacturer = "Comelit", model = "Comelit", name = "Comelit Digital Output", suggested_area = dev.AreaName } }.ToString();
            dev.ConfigTopic = $"homeassistant/switch/{dev.GetIDForTopic()}/config";
            dev.ConfigReadyToSend = true;
            dev.CanSubscribe = true;
        }
        public static void CreateClimaConfig(DevicesStructure.Clima dev)
        {
            dev.CommandTopic = "";
            dev.StatusTopic = "";
            dev.ConfigPayload = "";
            dev.ConfigTopic = "";
        }
        public static void CreateIrrigationValveConfig(DevicesStructure.IrrigationValve dev)
        {
            //configurazione come switch mdi:pipe-valve
            dev.CommandTopic = $"home/valves/{dev.GetIDForTopic()}/set";
            dev.StatusTopic = $"home/valves/{dev.GetIDForTopic()}/state";
            dev.ConfigPayload = new MQTTConfig() { name = dev.Description, device_class = "switch", object_id = dev.ID, unique_id = dev.ID, icon = "mdi:pipe-valve", command_topic = dev.CommandTopic, state_topic = dev.StatusTopic, device = new MQTTDevice() { identifiers = dev.AreaName, manufacturer = "Comelit", model = "Comelit", name = "Comelit Valve", suggested_area = dev.AreaName } }.ToString();
            dev.ConfigTopic = $"homeassistant/switch/{dev.GetIDForTopic()}/config";
            dev.ConfigReadyToSend = true;
            dev.CanSubscribe = true;
        }

        public static void CreateInputConfig(DevicesStructure.Input dev)
        {
            dev.StatusTopic = $"home/inputs/{dev.GetIDForTopic()}/state";
            dev.ConfigPayload = new MQTTConfig() { name = dev.Description, object_id = dev.ID, unique_id = dev.ID, state_topic = dev.StatusTopic, device = new MQTTDevice() { identifiers = dev.AreaName, manufacturer = "Comelit", model = "Comelit", name = "Comelit Input", suggested_area = dev.AreaName } }.ToString();
            dev.ConfigTopic = $"homeassistant/binary_sensor/{dev.GetIDForTopic()}/config";
            dev.ConfigReadyToSend = true;
            dev.CanSubscribe = true;
        }

        internal static void CreateAlarmConfig(DevicesStructure.Input dev)
        {
            dev.StatusTopic = $"home/inputs/{dev.GetIDForTopic()}/state";
            dev.ConfigPayload = new MQTTConfig() { name = dev.Description, object_id = dev.ID, unique_id = dev.ID, state_topic = dev.StatusTopic, device = new MQTTDevice() { identifiers = dev.AreaName, manufacturer = "Comelit", model = "Comelit", name = "Comelit Alarm", suggested_area = dev.AreaName } }.ToString();
            dev.ConfigTopic = $"homeassistant/binary_sensor/{dev.GetIDForTopic()}/config";
            dev.ConfigReadyToSend = true;
            dev.CanSubscribe = true;
        }
    }
}

namespace MQTT_NET_COMELIT.Comelit.DevicesStructure
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
            return ID.Replace("#", "hash").Replace(".", "dot");
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