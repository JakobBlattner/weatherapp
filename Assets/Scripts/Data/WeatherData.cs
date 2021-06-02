using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts.Data
{
    public class WeatherData
    {
        public DataType dataType{get; set;}
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

        //daily && current
        public int sunrise { get; set; }
        public int sunset { get; set; }

        //daily && hourly
        public float pop { get; set; }

        //current && hourly
        public float temp { get; set; }
        public float feels_like { get; set; }
        public Rain rain { get; set; }
        public Snow snow { get; set; }

        //daily
        public Temperature d_temp { get; set; }
        public Temperature d_feels_like { get; set; }
        public float d_rain { get; set; }
        public float d_snow { get; set; }
    }
}