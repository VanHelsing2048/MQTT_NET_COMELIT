namespace MQTT_NET_COMELIT.Utility
{
    public static class Utility
    {
        public enum LogLevel
        {
            Debug = 0,
            Info = 1,
            Warning = 2,
            Error = 3
        }

        private static LogLevel _minimumLogLevel = LogLevel.Info;

        public static void SetLogLevel(string logLevel)
        {
            if (Enum.TryParse(logLevel, true, out LogLevel parsedLevel))
            {
                _minimumLogLevel = parsedLevel;
                return;
            }

            _minimumLogLevel = LogLevel.Info;
            WriteLog($"Invalid log-level '{logLevel}', using info", LogLevel.Warning);
        }

        public static void WriteLog(string message, LogLevel level = LogLevel.Info)
        {
            if (level < _minimumLogLevel)
            {
                return;
            }

            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss:fff} - {level.ToString().ToUpperInvariant()} - {message}");
        }

        public static string NormalizeComelitTemperature(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            if (!double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double temperature))
            {
                return value;
            }

            if (Math.Abs(temperature) >= 100)
            {
                temperature /= 10.0;
            }

            return temperature.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
        }

        public static string ToComelitTemperatureValue(double celsius)
        {
            return ((int)Math.Round(celsius * 10.0)).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
