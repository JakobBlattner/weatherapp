using Assets.Scripts.Data;
using Newtonsoft.Json;
using System;
using UnityEngine;

public class WeatherService : MonoBehaviour
{

    //WeatherIcon Sprites
    public Sprite thunderSprite;
    public Sprite[] rainSprites = new Sprite[4];
    public Sprite[] snowSprites = new Sprite[3];
    public Sprite freezeSprite;
    public Sprite fogSprite;
    public Sprite[] cloudsSprites = new Sprite[4];
    public Sprite[] sunMoonSprites = new Sprite[4];

    private int timezoneOffset = 0;

    private WeatherBackend data;
    void Awake()
    {
        data = new WeatherBackend();
    }

    /// <summary>
    /// Converts string from backend to WeatherData object and returns it.
    /// </summary>
    /// <returns>WeatherData object containing information about current weather.</returns>
    public WeatherResponse GetCurrentWeather()
    {
        try
        {
            string stringdata = data.GetCurrentWeather();
            stringdata = ReplaceUnsuitableSubstrings(stringdata);
            WeatherResponse weatherdata = JsonConvert.DeserializeObject<WeatherResponse>(stringdata);

            //sets timezone offset if not already set to some value
            if (timezoneOffset == 0)
                timezoneOffset = weatherdata.timezone_offset;

            return weatherdata;
        }
        catch (JsonException ex)
        {
            Debug.LogError("Getting current weather data failed: " + ex.Message + "\n" + ex.StackTrace);
            return null;
        }
    }

    /// <summary>
    /// Converts string from backend to ForecastWeatherData object and returns it.
    /// </summary>
    /// <returns>ForecastWeatherData object containing information about upcoming weather.</returns>
    public WeatherResponse GetDailyForecastWeather()
    {
        try
        {
            string stringdata = data.GetDailyForecastWeather();
            stringdata = ReplaceUnsuitableSubstrings(stringdata);
            stringdata = stringdata.Replace("feels_like\":", "d_feels_like\":");
            stringdata = stringdata.Replace("temp\":", "d_temp\":");
            stringdata = stringdata.Replace("rain\":", "d_rain\":");
            stringdata = stringdata.Replace("snow\":", "d_snow\":");
            WeatherResponse weatherdata = JsonConvert.DeserializeObject<WeatherResponse>(stringdata);
            return weatherdata;
        }
        catch (JsonException ex)
        {
            Debug.LogError("Getting daily forecast weather data failed: " + ex.Message + "\n" + ex.StackTrace);
            return null;
        }
    }

    /// <summary>
    /// Gets data of whole current day and returns it 
    /// </summary>
    /// <returns>WeatherResponse with HourlyWeatherData for weather of current day</returns>
    public WeatherResponse GetWeatherOfNext48Hours()
    {
        try
        {
            string stringdata = data.GetHourlyForecastWeather();
            stringdata = ReplaceUnsuitableSubstrings(stringdata);
            WeatherResponse weatherData = JsonConvert.DeserializeObject<WeatherResponse>(stringdata);

            //seconds call to previous day because bug in rest backend: timezoneoffset is not being applied to start of day (always starts at 2 o'clock)
            TimeSpan span = DateTime.Now - new DateTime(1970, 1, 1);
            string historicaData = data.GetHistoricaWeatherData(((int)span.TotalSeconds - timezoneOffset - 3600 * 24).ToString());
            WeatherResponse historicWeatherResponse = JsonConvert.DeserializeObject<WeatherResponse>(historicaData);
            weatherData.hourly.Add(historicWeatherResponse.hourly[historicWeatherResponse.hourly.Count - 1]);
            weatherData.hourly.Add(historicWeatherResponse.hourly[historicWeatherResponse.hourly.Count - 2]);

            return weatherData;
        }
        catch (JsonException ex)
        {
            Debug.LogError("Getting weather data for next 48 hours failed: " + ex.Message + "\n" + ex.StackTrace);
            return null;
        }
    }

    /// <summary>
    /// //replaces strings which are not sutable for c# naming conventions
    /// </summary>
    /// <param name="unsuitable"></param>
    /// <returns>suitable string</returns>
    private string ReplaceUnsuitableSubstrings(string unsuitable)
    {
        unsuitable = unsuitable.Replace("event", "event_");
        unsuitable = unsuitable.Replace("1h", "oneH");

        return unsuitable;
    }

    /// <summary>
    /// Uses the weather id to choose icons displaying the current weather accordingly.
    /// See https://openweathermap.org/weather-conditions for weatherId values.
    /// </summary>
    /// <param name="weatherData">Weather data from the backend.</param>
    /// <returns>Array with five elements representing the current weather.</returns>
    public Sprite[] GetWeatherIcons(WeatherData weatherData)
    {
        Sprite[] weatherSprites = new Sprite[5];
        int weatherId = weatherData.weather[0].id;

        //thunderImage.sprite = weatherSprites[0];
        if ((int)(weatherId / 100) == 2)
        {
            weatherSprites[0] = thunderSprite;
        }

        //conditionImage.sprite = weatherSprites[1];
        //drizzle
        if ((int)(weatherId / 100) == 3)
        {
            weatherSprites[1] = rainSprites[0];
        }
        //rain
        else if ((int)(weatherId / 100) == 5)
        {
            if (weatherId == 500)
            {
                weatherSprites[1] = rainSprites[1];
            }
            else if (weatherId == 501)
            {
                weatherSprites[1] = rainSprites[2];
            }
            else
            {
                weatherSprites[1] = rainSprites[3];
            }
        }
        //snow
        else if ((int)(weatherId / 100) == 6)
        {
            //light snow
            if (weatherId == 600 || weatherId == 612 || weatherId == 615 || weatherId == 620)
            {
                weatherSprites[1] = snowSprites[0];
            }
            //heavy snow
            else if (weatherId == 602 || weatherId == 622)
            {
                weatherSprites[1] = snowSprites[2];
            }
            //medium/ other snow
            else
                weatherSprites[1] = snowSprites[1];
        }
        //fog/ mist
        else if ((int)(weatherId / 100) == 7)
        {
            weatherSprites[1] = fogSprite;
        }

        //freezeImage.sprite = weatherSprites[2];
        if ((weatherData.dataType == DataType.Daily && ((DailyWeatherData)weatherData).d_temp.min < 1) || (weatherData.dataType == DataType.Current && ((CurrentWeatherData)weatherData).temp < 1) || (weatherData.dataType == DataType.Hourly && ((HourlyWeatherData)weatherData).temp < 1))
        {
            weatherSprites[2] = freezeSprite;
        }

        //cloudsImage.sprite = weatherSprites[3];
        int cloudiness = weatherData.clouds;
        if (cloudiness > 10)
        {
            //set cloud sprite depending on cloudiness
            if (cloudiness < 25)
                weatherSprites[3] = cloudsSprites[0];
            else if (cloudiness < 50)
                weatherSprites[3] = cloudsSprites[1];
            else if (cloudiness < 75)
                weatherSprites[3] = cloudsSprites[2];
            else
                weatherSprites[3] = cloudsSprites[3];
        }

        //sunMoonImage.sprite = weatherSprites[4];
        if ((int)(weatherId / 100) == 8)
        {
            //DateTime sunrise = new DateTime(1970, 1, 1).AddSeconds(weatherData.sunrise + timeOffset);
            //DateTime sunset = new DateTime(1970, 1, 1).AddSeconds(weatherData.sunset + timeOffset);

            //if is between sunrise and sunset, set sun, otherwise moon
            if ((weatherData.dataType == DataType.Current || weatherData.dataType == DataType.Daily) && DateTime.Compare(DateTime.Now, new DateTime(1970, 1, 1).AddSeconds(weatherData.sunrise + timezoneOffset)) < 0 && DateTime.Compare(DateTime.Now, new DateTime(1970, 1, 1).AddSeconds(weatherData.sunset + timezoneOffset)) > 0)
            {
                //sets big moon if weather is clear
                if (weatherId == 800)
                {
                    weatherSprites[4] = sunMoonSprites[2];
                }
                //sets small moon otherwise
                else
                {
                    weatherSprites[4] = sunMoonSprites[3];
                }
            }
            else
            {
                //sets big sun if weather is clear
                if (weatherId == 800)
                    {
                    weatherSprites[4] = sunMoonSprites[0];
                }
                //sets small sun otherwise
                else
                {
                    weatherSprites[4] = sunMoonSprites[1];
                }
            }
        }

        return weatherSprites;
    }
}
