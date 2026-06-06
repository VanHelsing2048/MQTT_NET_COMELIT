namespace MQTT_NET_COMELIT.Comelit
{
    /// <summary>
    /// Centralized MQTT topic patterns for all device types
    /// </summary>
    public static class TopicPatterns
    {
        // Base paths
        public const string BasePath = "home";
        public const string DiscoveryPrefix = "homeassistant";

        // Light topics
        public const string LightCommandTopic = $"{BasePath}/lights/{{0}}/set";
        public const string LightStateTopic = $"{BasePath}/lights/{{0}}/state";
        public const string LightBrightnessSetTopic = $"{BasePath}/lights/{{0}}/brightness/set";
        public const string LightBrightnessStateTopic = $"{BasePath}/lights/{{0}}/brightness/state";
        public const string LightDiscoveryTopic = $"{DiscoveryPrefix}/light/{{0}}/config";

        // Cover/Blind topics
        public const string CoverCommandTopic = $"{BasePath}/cover/{{0}}/set";
        public const string CoverStateTopic = $"{BasePath}/cover/{{0}}/state";
        public const string CoverPositionSetTopic = $"{BasePath}/cover/{{0}}/position/set";
        public const string CoverPositionStateTopic = $"{BasePath}/cover/{{0}}/position/state";
        public const string CoverDiscoveryTopic = $"{DiscoveryPrefix}/cover/{{0}}/config";

        // Switch/Valve topics
        public const string SwitchCommandTopic = $"{BasePath}/switch/{{0}}/set";
        public const string SwitchStateTopic = $"{BasePath}/switch/{{0}}/state";
        public const string SwitchDiscoveryTopic = $"{DiscoveryPrefix}/switch/{{0}}/config";

        // Valve topics
        public const string ValveCommandTopic = $"{BasePath}/valves/{{0}}/set";
        public const string ValveStateTopic = $"{BasePath}/valves/{{0}}/state";
        public const string ValveDiscoveryTopic = $"{DiscoveryPrefix}/switch/{{0}}/config";

        // Climate topics
        public const string ClimateCommandTopic = $"{BasePath}/climate/{{0}}/set";
        public const string ClimateStateTopic = $"{BasePath}/climate/{{0}}/state";
        public const string ClimateCurrentTemperatureTopic = $"{BasePath}/climate/{{0}}/current-temperature/state";
        public const string ClimateTargetTemperatureSetTopic = $"{BasePath}/climate/{{0}}/target-temperature/set";
        public const string ClimateTargetTemperatureStateTopic = $"{BasePath}/climate/{{0}}/target-temperature/state";
        public const string ClimateCurrentHumidityTopic = $"{BasePath}/climate/{{0}}/current-humidity/state";
        public const string ClimateTargetHumiditySetTopic = $"{BasePath}/climate/{{0}}/target-humidity/set";
        public const string ClimateTargetHumidityStateTopic = $"{BasePath}/climate/{{0}}/target-humidity/state";
        public const string ClimateActionTopic = $"{BasePath}/climate/{{0}}/action/state";
        public const string ClimateDiscoveryTopic = $"{DiscoveryPrefix}/climate/{{0}}/config";

        // Input/Sensor topics
        public const string InputStateTopic = $"{BasePath}/inputs/{{0}}/state";
        public const string InputDiscoveryTopic = $"{DiscoveryPrefix}/binary_sensor/{{0}}/config";

        // Availability topics
        public const string AvailabilityTopic = $"{BasePath}/inputs/comelit-available/state";
        public const string PayloadAvailable = "ON";
        public const string PayloadNotAvailable = "OFF";

        /// <summary>
        /// Format a topic pattern with a device ID
        /// </summary>
        public static string Format(string pattern, string deviceId) => string.Format(pattern, deviceId);
    }
}
