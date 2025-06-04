using Newtonsoft.Json;
using static MQTT_NET_COMELIT.Comelit.Enums;

namespace MQTT_NET_COMELIT.Comelit
{
    public class AnnounceMessage
    {
        [JsonProperty("req_type")] public REQUEST_TYPE ReqType { get; set; } = REQUEST_TYPE.ANNOUNCE;
        [JsonProperty("seq_id")] public int SeqId { get; set; } = 1;
        [JsonProperty("req_sub_type")] public REQUEST_SUB_TYPE ReqSubType { get; set; } = REQUEST_SUB_TYPE.NONE;
        [JsonProperty("agent_type")] public int AgentType { get; set; } = 0;
    }

    public class LoginMessage
    {
        [JsonProperty("req_type")] public REQUEST_TYPE ReqType { get; set; } = REQUEST_TYPE.LOGIN;
        [JsonProperty("seq_id")] public int SeqId { get; set; } = 2;
        [JsonProperty("req_sub_type")] public REQUEST_SUB_TYPE ReqSubType { get; set; } = REQUEST_SUB_TYPE.NONE;
        [JsonProperty("agent_type")] public int AgentType { get; set; } = 0;
        [JsonProperty("agent_id")] public string AgentId { get; set; } = string.Empty;
        [JsonProperty("user_name")] public string UserName { get; set; } = "admin";
        [JsonProperty("password")] public string Password { get; set; } = "admin";
    }

    public class StatusMessage
    {
        [JsonProperty("req_type")] public REQUEST_TYPE ReqType { get; set; } = REQUEST_TYPE.STATUS;
        [JsonProperty("seq_id")] public int SeqId { get; set; } = 3;
        [JsonProperty("req_sub_type")] public REQUEST_SUB_TYPE ReqSubType { get; set; } = REQUEST_SUB_TYPE.NONE;
        [JsonProperty("sessiontoken")] public string SessionToken { get; set; } = string.Empty;
        [JsonProperty("obj_id")] public string ObjId { get; set; } = string.Empty;
        [JsonProperty("detail_level")] public int DetailLevel { get; set; } = 1;
    }

    public class SubscribeMessage
    {
        [JsonProperty("req_type")] public REQUEST_TYPE ReqType { get; set; } = REQUEST_TYPE.SUBSCRIBE;
        [JsonProperty("seq_id")] public int SeqId { get; set; } = 4;
        [JsonProperty("req_sub_type")] public REQUEST_SUB_TYPE ReqSubType { get; set; } = REQUEST_SUB_TYPE.SUBSCRIBE_RT;
        [JsonProperty("sessiontoken")] public string SessionToken { get; set; } = string.Empty;
        [JsonProperty("obj_id")] public string ObjId { get; set; } = string.Empty;
    }

    public class ActionCommandMessage
    {
        [JsonProperty("req_type")] public REQUEST_TYPE ReqType { get; set; } = REQUEST_TYPE.ACTION;
        [JsonProperty("seq_id")] public int SeqId { get; set; } = 4;
        [JsonProperty("req_sub_type")] public REQUEST_SUB_TYPE ReqSubType { get; set; } = REQUEST_SUB_TYPE.SET_ACTION_OBJ;
        [JsonProperty("act_type")] public ACTION_TYPE ActType { get; set; } = ACTION_TYPE.SET;
        [JsonProperty("sessiontoken")] public string SessionToken { get; set; } = string.Empty;
        [JsonProperty("obj_id")] public string ObjId { get; set; } = string.Empty;
        [JsonProperty("act_params")] public string[] ActParams { get; set; } = Array.Empty<string>();
    }

    internal static class ComelitMessages
    {
        public static string Serialize(object message) => JsonConvert.SerializeObject(message);

        public static string BuildAnnounce() => Serialize(new AnnounceMessage());

        public static string BuildLogin(string agentId)
        {
            return Serialize(new LoginMessage { AgentId = agentId });
        }

        public static string BuildStatus(string token, string objId)
        {
            return Serialize(new StatusMessage { SessionToken = token, ObjId = objId });
        }

        public static string BuildSubscribe(string token, string objId)
        {
            return Serialize(new SubscribeMessage { SessionToken = token, ObjId = objId });
        }

        public static string BuildOnOffCommand(string token, string objId, string toggle)
        {
            return Serialize(new ActionCommandMessage { SessionToken = token, ObjId = objId, ActParams = new[] { toggle } });
        }
    }
}
