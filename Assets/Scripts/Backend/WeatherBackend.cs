using UnityEngine;
using System.Net;
using System;

public class WeatherBackend
{
    private const string API_KEY = "ec5555dcb3f29d976d30f0297923936b";
    //private const string CITY_ID = "2764359"; //city id for steyr, according to city.list.json.gz http://bulk.openweathermap.org/sample/
    private float lat = 48.037659f;
    private float lon = 14.396160f;
    private string currentUrl;
    private string dailyForecastUrl;
    private string hourlyForecastUrl;
    private string historictUrl;

    public WeatherBackend()
    {
        currentUrl = "https://api.openweathermap.org/data/2.5/onecall?lat=" +
                lat + "&lon=" + lon + "&exclude=hourly,minutely,daily&units=metric&appid=" + API_KEY;
        dailyForecastUrl = "https://api.openweathermap.org/data/2.5/onecall?lat=" +
                lat + "&lon=" + lon + "&exclude=hourly,minutely,current,alerts&units=metric&appid=" + API_KEY;
        hourlyForecastUrl = "https://api.openweathermap.org/data/2.5/onecall?lat=" +
                lat + "&lon=" + lon + "&exclude=daily,minutely,alerts,current&units=metric&appid=" + API_KEY;
        historictUrl = "https://api.openweathermap.org/data/2.5/onecall/timemachine?lat=" +
                +lat + "&lon=" + lon + "&dt={timeString}&units=metric&appid=" + API_KEY;
    }

    public string GetDailyForecastWeather()
    {
        // Create a web client.
        using (WebClient client = new WebClient())
        {

            // Get the response string from the URL.
            try
            {
                return client.DownloadString(dailyForecastUrl);
            }
            catch (WebException ex)
            {
                Debug.LogError("Webexception: " + ex.Message + " " + ex.StackTrace);
                return "";
            }
            catch (Exception ex)
            {
                Debug.LogError("Unknown error\n" + ex.Message + " " + ex.StackTrace);
                return "";
            }
        }
    }

    public string GetHourlyForecastWeather()
    {
        using (WebClient client = new WebClient())
        {
            try
            {
                return client.DownloadString(hourlyForecastUrl);
            }
            catch (WebException ex)
            {
                Debug.LogError("Webexception: " + ex.Message + " " + ex.StackTrace);
                return "";
            }
            catch (Exception ex)
            {
                Debug.LogError("Unknown error\n" + ex.Message + " " + ex.StackTrace);
                return "";
            }
        }
    }

    public string GetHistoricaWeatherData(string timeInSeconds)
    {
        using (WebClient client = new WebClient())
        {
            try
            {
                //replaces placeholder characters in historicUrl
                string currentHistoricUrl = historictUrl.Replace("{timeString}", timeInSeconds);
                return client.DownloadString(currentHistoricUrl);
            }
            catch (WebException ex)
            {
                Debug.LogError("Webexception: " + ex.Message + " " + ex.StackTrace);
                return "";
            }
            catch (Exception ex)
            {
                Debug.LogError("Unknown error\n" + ex.Message + " " + ex.StackTrace);
                return "";
            }
        }
    }

    public string GetCurrentWeather()
    {
        // Create a web client.
        using (WebClient client = new WebClient())
        {
            // Get the response string from the URL.
            try
            {
                return client.DownloadString(currentUrl);
            }
            catch (WebException ex)
            {
                Debug.LogError("Webexception: " + ex.Message + " " + ex.StackTrace);
                return "";
            }
            catch (Exception ex)
            {
                Debug.LogError("Unknown error\n" + ex.Message + " " + ex.StackTrace);
                return "";
            }
        }
    }
}
