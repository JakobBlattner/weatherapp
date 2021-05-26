using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Data
{
    public class HourlyWeatherData : WeatherData
    {
        public float temp { get; set; }
        public float feels_like { get; set; }
        public float pop { get; set; }
        public Rain rain { get; set; }
        public Snow snow{ get; set; }
    }
}