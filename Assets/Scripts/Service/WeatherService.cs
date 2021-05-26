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
            Debug.LogError(ex.Message + " " + ex.StackTrace);
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
            WeatherResponse weatherdata = JsonConvert.DeserializeObject<WeatherResponse>(stringdata);
            return weatherdata;
        }
        catch (JsonException ex)
        {
            Debug.LogError(ex.Message + " " + ex.StackTrace);
            return null;
        }
    }

    /// <summary>
    /// Gets data of whole current day and returns it 
    /// </summary>
    /// <returns>WeatherResponse with HourlyWeatherData for weather of current day</returns>
    public WeatherResponse GetHourlyWeatherOfCurrentDay()
    {
        try
        {
            //get historic data
            TimeSpan span = DateTime.Now - new DateTime(1970, 1, 1);
            string historicaData = data.GetHistoricaWeatherData(((int)span.TotalSeconds - timezoneOffset - 1).ToString());
            WeatherResponse weatherData = JsonConvert.DeserializeObject<WeatherResponse>(historicaData);
            weatherData.hourly. Remove(weatherData.hourly[weatherData.hourly.Count - 1]); //removes current hour

            if (weatherData != null)
            {
                //seconds call to previous day because bug in rest backend: timezoneoffset is not being applied to start of day (always starts at 2 o'clock)
                historicaData = data.GetHistoricaWeatherData(((int)span.TotalSeconds - timezoneOffset - 3600 * 24).ToString());
                WeatherResponse historicWeatherResponse = JsonConvert.DeserializeObject<WeatherResponse>(historicaData);
                weatherData.hourly.Insert(0, historicWeatherResponse.hourly[historicWeatherResponse.hourly.Count - 1]);
                weatherData.hourly.Insert(0, historicWeatherResponse.hourly[historicWeatherResponse.hourly.Count - 2]);

                //get forecastweather and append to weatherData list
                string stringdata = data.GetHourlyForecastWeather();
                stringdata = ReplaceUnsuitableSubstrings(stringdata);
                weatherData.hourly.AddRange(JsonConvert.DeserializeObject<WeatherResponse>(stringdata).hourly);

                /*foreach (HourlyWeatherData hourly in weatherData.hourly)
                {
                    Debug.Log(new DateTime(1970, 1, 1).AddSeconds(hourly.dt + weatherData.timezone_offset).ToString("HH:mm dd.MM.yyyy"));
                }*/
            }

            return weatherData;
        }
        catch (JsonException ex)
        {
            Debug.LogError(ex.Message + " " + ex.StackTrace);
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
    public Sprite[] GetWeatherIcons(WeatherData weatherData, int timeOffset, bool dailyWeatherData)
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
        if ((dailyWeatherData && ((DailyWeatherData)weatherData).temp.min < 1) || !dailyWeatherData && ((CurrentWeatherData)weatherData).temp < 1)
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
        if (weatherId == 800 || weatherId == 801)
        {
            //DateTime sunrise = new DateTime(1970, 1, 1).AddSeconds(weatherData.sunrise + timeOffset);
            //DateTime sunset = new DateTime(1970, 1, 1).AddSeconds(weatherData.sunset + timeOffset);

            //if is between sunrise and sunset, set sun, otherwise moon
            if (!dailyWeatherData && DateTime.Compare(DateTime.Now, new DateTime(1970, 1, 1).AddSeconds(((CurrentWeatherData)weatherData).sunrise + timeOffset)) < 0 && DateTime.Compare(DateTime.Now, new DateTime(1970, 1, 1).AddSeconds(((CurrentWeatherData)weatherData).sunset + timeOffset)) > 0)
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
