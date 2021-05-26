using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Data
{
    public class CurrentWeatherData : WeatherData
    {
        public float temp { get; set; }
        public float feels_like { get; set; }
        public int sunrise { get; set; }
        public int sunset { get; set; }
        public Rain rain { get; set; }
        public Snow snow{ get; set; }
    }
}