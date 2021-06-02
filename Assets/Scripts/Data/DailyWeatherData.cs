namespace Assets.Scripts.Data
{
    public class DailyWeatherData : WeatherData
    {
        public DailyWeatherData()
        {
            dataType = DataType.Daily;
        }

        public int moonrise { get; set; }
        public int moonset { get; set; }
        public float moonphase { get; set; }
    }
}