using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Data
{
    public class HourlyWeatherData : WeatherData
    {
        public HourlyWeatherData()
        {
            dataType = DataType.Hourly;
        }
    }
}