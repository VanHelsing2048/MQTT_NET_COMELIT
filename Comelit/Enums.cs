using System.Text.Json;
using System.Text.Json.Serialization;
using static MQTT_NET_COMELIT.Comelit.Enums;

namespace MQTT_NET_COMELIT.Comelit
{
    public static class Enums
    {
        public enum REQUEST_TYPE
        {
            STATUS = 0,
            ACTION = 1,
            SUBSCRIBE = 3,
            LOGIN = 5,
            PING = 7,
            READ_PARAMS = 8,
            GET_DATETIME = 9,
            ANNOUNCE = 13
        }

        public enum REQUEST_SUB_TYPE
        {
            CREATE_OBJ,
            UPDATE_OBJ,
            DELETE_OBJ,
            SET_ACTION_OBJ,
            GET_TEMPO_OBJ,
            SUBSCRIBE_RT,
            UNSUBSCRIBE_RT,
            GET_CONF_PARAM_GROUP = 23,
            NONE = -1
        }

        public enum ACTION_TYPE
        {
            SET,
            CLIMA_MODE,
            CLIMA_SET_POINT,
            SWITCH_SEASON = 4,
            SWITCH_CLIMA_MODE = 13,
            UMI_SETPOINT = 19,
            SWITCH_UMI_MODE = 23,
            SET_BLIND_POSITION = 52
        }

        public enum DEVICE_STATUS
        {
            OFF = '0',
            ON = '1',
            IDLE = '2'
        }

        public enum OBJECT_TYPE
        {
            OTHER = 1,
            BLIND = 2,
            LIGHT = 3,
            IRRIGATION = 4,
            AUTOMATION = 5,
            THERMOSTAT = 9,
            OUTLET = 10,
            POWER_SUPPLIER = 11,
            INPUT = 13,
            SCENARIO = 1000,
            ZONE = 1001,
            RULE = 1002
        }

        public enum OBJECT_SUBTYPE
        {
            GENERIC = 0,
            DIGITAL_LIGHT = 1,
            RGB_LIGHT = 2,
            TEMPORIZED_LIGHT = 3,
            DIMMER_LIGHT = 4,
            OTHER_DIGIT = 5,
            OTHER_TMP = 6,
            ELECTRIC_BLIND = 7,
            IRRIGATION_VALVE = 8,
            AUTOMATION = 10,
            CLIMA_TERM = 12,
            GENERIC_ZONE = 13,
            COUNTER = 14,
            CONSUMPTION = 15,
            CLIMA_THERMOSTAT_DEHUMIDIFIER = 16,
            CLIMA_DEHUMIDIFIER = 17,
            ENHANCED_ELECTRIC_BLIND = 31,
        }


    }

    public static class JsonParsing
    {
        public class Header
        {
            [Newtonsoft.Json.JsonProperty("out_data")]
            public List<OutData> OutData { get; set; }
            [Newtonsoft.Json.JsonProperty("sessiontoken")]
            public string SessionToken { get; set; }
            [Newtonsoft.Json.JsonProperty("seq_id")]
            public string SeqID { get; set; }
            [Newtonsoft.Json.JsonProperty("message")]
            public string Message { get; set; }
            [Newtonsoft.Json.JsonProperty("obj_id")]
            public string ObjID { get; set; }
        }

        public class OutData
        {
            public List<Element> Elements { get; set; }

            [Newtonsoft.Json.JsonProperty("agent_id")]
            public string AgentID { get; set; }
            [Newtonsoft.Json.JsonProperty("descrizione")]
            public string Descrizione { get; set; }
            [Newtonsoft.Json.JsonProperty("id")]
            public string ID { get; set; }
            [Newtonsoft.Json.JsonProperty("type")]
            public OBJECT_TYPE Type { get; set; }
            [Newtonsoft.Json.JsonProperty("sub_type")]
            public OBJECT_SUBTYPE SubType { get; set; }
            [Newtonsoft.Json.JsonProperty("status")]
            public string Status { get; set; }
            [Newtonsoft.Json.JsonProperty("powerst")]
            public string PowerSt { get; set; }
            [Newtonsoft.Json.JsonProperty("bright")]
            public string Bright { get; internal set; }
            [Newtonsoft.Json.JsonProperty("open_status")]
            public string OpenStatus { get; internal set; }
            [Newtonsoft.Json.JsonProperty("temperatura")]
            public string Temperatura { get; internal set; }
            [Newtonsoft.Json.JsonProperty("umidita")]
            public string Umidita { get; internal set; }
            [Newtonsoft.Json.JsonProperty("soglia_attiva")]
            public string SogliaAttiva { get; internal set; }
            [Newtonsoft.Json.JsonProperty("soglia_attiva_umi")]
            public string SogliaAttivaUmi { get; internal set; }
            [Newtonsoft.Json.JsonProperty("est_inv")]
            public string EstInv { get; internal set; }
            [Newtonsoft.Json.JsonProperty("auto_man")]
            public string AutoMan { get; internal set; }
            [Newtonsoft.Json.JsonProperty("auto_man_umi")]
            public string AutoManUmi { get; internal set; }
            [Newtonsoft.Json.JsonProperty("semiauto_enabled")]
            public string SemiautoEnabled { get; internal set; }
            [Newtonsoft.Json.JsonProperty("instant_power")]
            public string InstantPower { get; internal set; }
            [Newtonsoft.Json.JsonProperty("label_value")]
            public string LabelValue { get; internal set; }

            public override string ToString()
            {
                return Descrizione;
            }
        }

        public class Element
        {
            [Newtonsoft.Json.JsonProperty("id")]
            public string ID { get; set; }
            [Newtonsoft.Json.JsonProperty("data")]
            public ElementData Data { get; set; }
            public override string ToString()
            {
                return $"ID:{ID}";
            }
        }

        public class RuleAtom
        {
            [Newtonsoft.Json.JsonProperty("param_type")]
            public string ParamType { get; set; }
            [Newtonsoft.Json.JsonProperty("obj_id")]
            public string ObjID { get; set; }
            [Newtonsoft.Json.JsonProperty("condition")]
            public string Condition { get; set; }
            [Newtonsoft.Json.JsonProperty("value")]
            public string Value { get; set; }
            [Newtonsoft.Json.JsonProperty("description")]
            public string Description { get; set; }
            [Newtonsoft.Json.JsonProperty("status")]
            public string Status { get; set; }
        }

        //public class ElementData
        //{
        //    [Newtonsoft.Json.JsonProperty("id")]
        //    public string ID { get; set; }
        //    [Newtonsoft.Json.JsonProperty("type")]
        //    public OBJECT_TYPE Type { get; set; }
        //    [Newtonsoft.Json.JsonProperty("sub_type")]
        //    public OBJECT_SUBTYPE SubType { get; set; }
        //    [Newtonsoft.Json.JsonProperty("descrizione")]
        //    public string Descrizione { get; set; }
        //    [Newtonsoft.Json.JsonProperty("status")]
        //    public string Status { get; set; }
        //    [Newtonsoft.Json.JsonProperty("powerst")]
        //    public DEVICE_STATUS PowerSt { get; set; }
        //    public List<Element> Elements { get; set; }

        //    public override string ToString()
        //    {
        //        return $"ID:{Descrizione} - Type:{Type} - SubType:{SubType}";
        //    }
        //}

        public class ElementData
        {
            public List<Element> Elements { get; set; }
            [Newtonsoft.Json.JsonProperty("rule_atoms")]
            public List<RuleAtom> RuleAtoms { get; set; }
            public override string ToString()
            {
                return $"ID:{Description} - Type:{Type} - SubType:{SubType}";
            }

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
            [Newtonsoft.Json.JsonProperty("instant_power")]
            public string InstantPower { get; set; }
            [Newtonsoft.Json.JsonProperty("label_value")]
            public string LabelValue { get; set; }
            [Newtonsoft.Json.JsonProperty("visible")]
            public string Visible { get; set; }
            [Newtonsoft.Json.JsonProperty("prod")]
            public string Prod { get; set; }
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
            [Newtonsoft.Json.JsonProperty("openTime")]
            public string OpenTime { get; set; }
            [Newtonsoft.Json.JsonProperty("closeTime")]
            public string CloseTime { get; set; }
            [Newtonsoft.Json.JsonProperty("open_status")]
            public string OpenStatus { get; set; }
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

    public class Home : MainProperties
    {
        public List<Area> Areas = [];
        public override string ToString()
        {
            return $"ID:{ID} - Name:{Name}";
        }
    }

    public class Area : MainProperties
    {
        public List<Device> Devices { get; set; }
        public override string ToString()
        {
            return $"ID:{ID} - Name:{Name} - Devices:{Devices?.Count}";
        }
    }

    public abstract class MainProperties
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public OBJECT_TYPE Type { get; set; }
        public OBJECT_SUBTYPE SubType { get; set; }

        public string GetIDForTopic()
        {
            return MQTT_NET_COMELIT.Utility.Utility.NormalizeComelitId(ID);
        }
    }

    public class MQTTConfig
    {
        // Basic properties
        public string Name { get; set; }
        public string ObjectId { get; set; }
        public string UniqueId { get; set; }
        public string DeviceClass { get; set; }
        public string Icon { get; set; }
        public string CommandTopic { get; set; }
        public string StateTopic { get; set; }
        public string UnitOfMeasurement { get; set; }
        public string StateClass { get; set; }
        public int OffDelay { get; set; }
        public MQTTDevice Device { get; set; }

        // Availability
        public string AvailabilityTopic { get; set; } = "home/inputs/comelit-available/state";
        public string PayloadAvailable { get; set; } = "ON";
        public string PayloadNotAvailable { get; set; } = "OFF";

        // Light-specific properties
        public string PayloadOn { get; set; }
        public string PayloadOff { get; set; }
        public string PayloadPress { get; set; }
        public string BrightnessTopic { get; set; }
        public string BrightnessStateTopic { get; set; }
        public string BrightnessCommandTopic { get; set; }
        public string BrightnessScale { get; set; }
        public string RgbTopic { get; set; }
        public string RgbStateTopic { get; set; }
        public string RgbCommandTopic { get; set; }

        // Climate-specific properties
        public string CurrentTemperatureTopic { get; set; }
        public string TemperatureStateTopic { get; set; }
        public string TemperatureCommandTopic { get; set; }
        public string ModeStateTopic { get; set; }
        public string ModeCommandTopic { get; set; }
        public string ActionTopic { get; set; }
        public string PresetModeStateTopic { get; set; }
        public List<string> PresetModes { get; set; }
        public string MinTemp { get; set; }
        public string MaxTemp { get; set; }
        public string TempStep { get; set; }
        public List<string> Modes { get; set; }
        public List<string> FanModes { get; set; }
        public string SwingMode { get; set; }
        public string CurrentHumidityTopic { get; set; }
        public string TargetHumidityCommandTopic { get; set; }
        public string TargetHumidityStateTopic { get; set; }

        // Cover-specific properties
        public string PayloadOpen { get; set; }
        public string PayloadClose { get; set; }
        public string PayloadStop { get; set; }
        public string PositionTopic { get; set; }
        public string PositionOpenValue { get; set; }
        public string PositionClosedValue { get; set; }
        public string TiltCommandTopic { get; set; }
        public string TiltStateTopic { get; set; }
        public string TiltMin { get; set; }
        public string TiltMax { get; set; }

        private readonly JsonSerializerOptions options = new()
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public override string ToString()
        {
            // Serializza come oggetto intermedio per inserire i campi fissi
            Dictionary<string, object> obj = new()
            {
                ["name"] = Name,
                ["object_id"] = MQTT_NET_COMELIT.Utility.Utility.NormalizeComelitId(ObjectId),
                ["unique_id"] = MQTT_NET_COMELIT.Utility.Utility.NormalizeComelitId(UniqueId),
                ["state_topic"] = StateTopic,
                ["unit_of_measurement"] = string.IsNullOrEmpty(UnitOfMeasurement) ? null : UnitOfMeasurement,
                ["state_class"] = string.IsNullOrEmpty(StateClass) ? null : StateClass,
                ["availability_topic"] = string.IsNullOrEmpty(AvailabilityTopic) ? null : AvailabilityTopic,
                ["payload_available"] = string.IsNullOrEmpty(PayloadAvailable) ? null : PayloadAvailable,
                ["payload_not_available"] = string.IsNullOrEmpty(PayloadNotAvailable) ? null : PayloadNotAvailable,
                ["off_delay"] = OffDelay > 0 ? OffDelay : null,
                ["device_class"] = string.IsNullOrEmpty(DeviceClass) ? null : DeviceClass,
                ["command_topic"] = string.IsNullOrEmpty(CommandTopic) ? null : CommandTopic,
                ["icon"] = string.IsNullOrEmpty(Icon) ? null : Icon,
                ["device"] = Device,

                // Light-specific
                ["payload_on"] = string.IsNullOrEmpty(PayloadOn) ? null : PayloadOn,
                ["payload_off"] = string.IsNullOrEmpty(PayloadOff) ? null : PayloadOff,
                ["payload_press"] = string.IsNullOrEmpty(PayloadPress) ? null : PayloadPress,
                ["brightness_topic"] = string.IsNullOrEmpty(BrightnessTopic) ? null : BrightnessTopic,
                ["brightness_state_topic"] = string.IsNullOrEmpty(BrightnessStateTopic) ? null : BrightnessStateTopic,
                ["brightness_command_topic"] = string.IsNullOrEmpty(BrightnessCommandTopic) ? null : BrightnessCommandTopic,
                ["brightness_scale"] = string.IsNullOrEmpty(BrightnessScale) ? null : BrightnessScale,
                ["rgb_topic"] = string.IsNullOrEmpty(RgbTopic) ? null : RgbTopic,
                ["rgb_state_topic"] = string.IsNullOrEmpty(RgbStateTopic) ? null : RgbStateTopic,
                ["rgb_command_topic"] = string.IsNullOrEmpty(RgbCommandTopic) ? null : RgbCommandTopic,

                // Climate-specific
                ["current_temperature_topic"] = string.IsNullOrEmpty(CurrentTemperatureTopic) ? null : CurrentTemperatureTopic,
                ["temperature_state_topic"] = string.IsNullOrEmpty(TemperatureStateTopic) ? null : TemperatureStateTopic,
                ["temperature_command_topic"] = string.IsNullOrEmpty(TemperatureCommandTopic) ? null : TemperatureCommandTopic,
                ["mode_state_topic"] = string.IsNullOrEmpty(ModeStateTopic) ? null : ModeStateTopic,
                ["mode_command_topic"] = string.IsNullOrEmpty(ModeCommandTopic) ? null : ModeCommandTopic,
                ["action_topic"] = string.IsNullOrEmpty(ActionTopic) ? null : ActionTopic,
                ["preset_mode_state_topic"] = string.IsNullOrEmpty(PresetModeStateTopic) ? null : PresetModeStateTopic,
                ["preset_modes"] = PresetModes?.Count > 0 ? PresetModes : null,
                ["min_temp"] = string.IsNullOrEmpty(MinTemp) ? null : MinTemp,
                ["max_temp"] = string.IsNullOrEmpty(MaxTemp) ? null : MaxTemp,
                ["temp_step"] = string.IsNullOrEmpty(TempStep) ? null : TempStep,
                ["modes"] = Modes?.Count > 0 ? Modes : null,
                ["fan_modes"] = FanModes?.Count > 0 ? FanModes : null,
                ["swing_mode"] = string.IsNullOrEmpty(SwingMode) ? null : SwingMode,
                ["current_humidity_topic"] = string.IsNullOrEmpty(CurrentHumidityTopic) ? null : CurrentHumidityTopic,
                ["target_humidity_command_topic"] = string.IsNullOrEmpty(TargetHumidityCommandTopic) ? null : TargetHumidityCommandTopic,
                ["target_humidity_state_topic"] = string.IsNullOrEmpty(TargetHumidityStateTopic) ? null : TargetHumidityStateTopic,

                // Cover-specific
                ["payload_open"] = string.IsNullOrEmpty(PayloadOpen) ? null : PayloadOpen,
                ["payload_close"] = string.IsNullOrEmpty(PayloadClose) ? null : PayloadClose,
                ["payload_stop"] = string.IsNullOrEmpty(PayloadStop) ? null : PayloadStop,
                ["position_topic"] = string.IsNullOrEmpty(PositionTopic) ? null : PositionTopic,
                ["position_open_value"] = string.IsNullOrEmpty(PositionOpenValue) ? null : PositionOpenValue,
                ["position_closed_value"] = string.IsNullOrEmpty(PositionClosedValue) ? null : PositionClosedValue,
                ["tilt_command_topic"] = string.IsNullOrEmpty(TiltCommandTopic) ? null : TiltCommandTopic,
                ["tilt_state_topic"] = string.IsNullOrEmpty(TiltStateTopic) ? null : TiltStateTopic,
                ["tilt_min"] = string.IsNullOrEmpty(TiltMin) ? null : TiltMin,
                ["tilt_max"] = string.IsNullOrEmpty(TiltMax) ? null : TiltMax,
            };

            // Rimuovi tutte le proprietà null
            var keysToRemove = obj.Where(x => x.Value == null).Select(x => x.Key).ToList();
            foreach (var key in keysToRemove)
            {
                obj.Remove(key);
            }

            return JsonSerializer.Serialize(obj, options);
        }
    }


    public class MQTTDevice
    {
        [JsonPropertyName("identifiers")]
        public List<string> Identifiers { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("model")]
        public string Model { get; set; }
        [JsonPropertyName("manufacturer")]
        public string Manufacturer { get; set; }
        [JsonPropertyName("suggested_area")]
        public string SuggestedArea { get; set; }
    }

}
