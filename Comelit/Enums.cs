using MQTT_NET_COMELIT.Comelit.DevicesStructure;
using static MQTT_NET_COMELIT.Comelit.Enums;
using static MQTT_NET_COMELIT.Comelit.JsonParsing;

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
            THERMOSTAT = 9,
            OUTLET = 10,
            POWER_SUPPLIER = 11,
            INPUT = 13,
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
            public DEVICE_STATUS PowerSt { get; set; }
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
        public List<Area> Areas = new();
        public override string ToString()
        {
            return $"ID:{ID} - Name:{Name}";
        }
    }

    public class Area : MainProperties
    {
        public List<DevicesStructure.Device> Devices { get; set; }
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
            return ID.Replace("#", "hash").Replace(".", "dot");
        }
    }

    public class MQTTConfig
    {
        public string name { get; set; }
        public string object_id { get; set; }
        public string unique_id { get; set; }
        public string device_class { get; set; }
        public string icon { get; set; }
        public string command_topic { get; set; }
        public string state_topic { get; set; }
        public string off_delay { get; set; }
        public MQTTDevice device { get; set; }

        public override string ToString()
        {
            string offdelay = "";
            if (!string.IsNullOrEmpty(off_delay))
            {
                offdelay = "\", \"off_delay\": \"" + off_delay;
            }
            string devclass = "";
            if (!string.IsNullOrEmpty(device_class))
            {
                devclass = "\", \"device_class\": \"" + device_class;
            }
            string command = "";
            if (!string.IsNullOrEmpty(command_topic))
            {
                command = "\", \"command_topic\": \"" + command_topic;
            }
            string micon = "";
            if (!string.IsNullOrEmpty(icon))
            {
                micon = "\", \"icon\": \"" + icon;
            }
            return "{\"name\": \"" + name + devclass + "\", \"object_id\": \"" + object_id + "\", \"unique_id\": \"" + unique_id + micon + command + offdelay + "\", \"state_topic\": \"" + state_topic + "\", \"device\": {" + device + "}";
        }
    }

    public class MQTTDevice
    {
        public string identifiers { get; set; }
        public string name { get; set; }
        public string model { get; set; }
        public string manufacturer { get; set; }
        public string suggested_area { get; set; }

        public override string ToString()
        {
            return "\"identifiers\": [\"" + identifiers + "\"], \"name\": \"" + name + "\", \"model\": \"" + model + "\", \"manufacturer\": \"" + manufacturer + "\", \"suggested_area\": \"" + suggested_area + "\"}";
        }
    }

}
