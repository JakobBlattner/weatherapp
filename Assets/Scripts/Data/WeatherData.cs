using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts.Data
{
    public class WeatherData
    {
        public int dt { get; set; }
        public int pressure { get; set; }
        public int humidity { get; set; }
        public float dew_point { get; set; }
        public int clouds { get; set; }
        public float wind_speed { get; set; }
        public int wind_deg { get; set; }
        public float wind_gust { get; set; }
        public List<Weather> weather { get; set; }
        public float uvi { get; set; }
    }

    public class Temperature
    {
        public float day { get; set; }
        public float min { get; set; }
        public float max { get; set; }
        public float night { get; set; }
        public float eve { get; set; }
        public float morn { get; set; }
    }

    public class Rain
    {
        public float oneH { get; set; }
    }

    public class Snow
    {
        public float oneH { get; set; }
    }

    public class Weather
    {
        public string description { get; set; }
        public string icon { get; set; }
        public string main { get; set; }
        public int id { get; set; }
    }


}