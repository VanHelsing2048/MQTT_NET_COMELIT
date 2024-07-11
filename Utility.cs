namespace MQTT_NET_COMELIT.Utility
{
    public static class Utility
    {
        public static void WriteLog(string message)
        {
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " - " + message);
        }
    }
}
