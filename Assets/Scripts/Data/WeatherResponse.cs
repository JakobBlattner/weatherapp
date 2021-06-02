using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts.Data
{
    public class WeatherResponse
    {
        public List<Alert> alerts { get; set; }
        public float lat { get; set; }
        public float lon { get; set; }
        public string timezone { get; set; }
        public int timezone_offset { get; set; }

        public List<DailyWeatherData> daily { get; set; }
        public CurrentWeatherData current{ get; set; }
        public List<HourlyWeatherData> hourly{ get; set; }
    }
}