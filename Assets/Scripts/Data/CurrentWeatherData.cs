using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Data
{
    public class CurrentWeatherData : WeatherData
    {
        public CurrentWeatherData()
        {
            dataType = DataType.Current;
        }
    }
}