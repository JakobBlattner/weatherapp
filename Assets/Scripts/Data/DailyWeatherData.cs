namespace Assets.Scripts.Data
{
    public class DailyWeatherData : WeatherData
    {
        public DailyWeatherData()
        {
            dataType = DataType.Daily;
        }

        public Temperature temp { get; set; }
        public Temperature feels_like { get; set; }
        public int moonrise { get; set; }
        public int moonset { get; set; }
        public float moonphase { get; set; }
        public int sunrise { get; set; }
        public int sunset { get; set; }
        public float pop { get; set; }
        public float rain { get; set; }
        public float snow { get; set; }
    }
}