using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using Assets.Scripts.Data;

public class WeatherDisplay : MonoBehaviour
{
    private WeatherService service;
    private const int minutesBetweenCurrentWeatherRequests = 1;
    private const int minutesBetweenForecastWeatherRequests = 60;
    private float currentWeatherUpdateTime;
    private float forecastWeatherUpdateTime;

    public AlertPopup alertPopup;
    public TextMeshProUGUI alertText;

    [Header("Current WeatherIcon Images")]
    public Image thunderImage;
    public Image conditionImage;
    public Image freezeImage;
    public Image cloudsImage;
    public Image sunMoonImage;

    [Header("GUI Text")]
    public TextMeshProUGUI currentTemp;
    public TextMeshProUGUI feelsLikeTemp;
    public TextMeshProUGUI description;
    public TextMeshProUGUI precipitation;
    public TextMeshProUGUI humidity;
    public TextMeshProUGUI wind;
    public TextMeshProUGUI sunrise;
    public TextMeshProUGUI sunset;
    public TextMeshProUGUI time;
    public TextMeshProUGUI location;
    public TextMeshProUGUI dayOfTheWeek;

    [Header("Forecast Prefabs")]
    public Transform dayForecastParent;
    public GameObject dailyForecastPrefab;
    public Transform hourForecastParent;
    public GameObject hourlyForecastPrefab;

    void Awake()
    {
        service = GetComponent<WeatherService>();
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            GetAndDisplayCurrentWeather();
            GetAndDisplayForecastWeather();
            GetAndDisplayCurrentDayHourlyWeather();
        }
    }

    //gets current weather every 10minutes and forecast weather every 60 minutes
    private void Update()
    {
        currentWeatherUpdateTime += Time.deltaTime;
        forecastWeatherUpdateTime += Time.deltaTime;
        if (currentWeatherUpdateTime > (minutesBetweenCurrentWeatherRequests * 60))
        {
            GetAndDisplayCurrentWeather();
        }
        if (forecastWeatherUpdateTime > (minutesBetweenForecastWeatherRequests * 60))
        {
            GetAndDisplayForecastWeather();
            GetAndDisplayCurrentDayHourlyWeather();
        }
    }

    private void GetAndDisplayCurrentWeather()
    {
        Debug.Log("Getting current weather data");
        currentWeatherUpdateTime = 0;
        WeatherResponse wal = service.GetCurrentWeather();
        DisplayCurrentWeather(wal);
        Debug.Log("Got current weather data, weatherId = " + wal.current.weather[0].id);
    }

    private void GetAndDisplayForecastWeather()
    {
        Debug.Log("Getting forecast weather data");
        forecastWeatherUpdateTime = 0;

        //daily forecast weather
        WeatherResponse wr = service.GetDailyForecastWeather();
        if (wr != null)
            DisplayDailyForecast(wr);
        else
            Debug.LogError("No daily forecast weather data received");
    }

    private void GetAndDisplayCurrentDayHourlyWeather()
    {
        Debug.Log("Getting 24h weather data of current day");

        //hourly current/forecast data
        WeatherResponse wr = service.GetHourlyWeatherOfCurrentDay();
        if (wr != null)
            DisplayHourlyWeather(wr);
        else
            Debug.LogError("No hourly forecast weather data received");
    }

    /// <summary>
    /// Displays current weather from the passed weatherdata.
    /// </summary>
    /// <param name="weatherdata"></param>
    private void DisplayCurrentWeather(WeatherResponse weatherdata)
    {
        if (weatherdata != null)
        {
            currentTemp.text = Mathf.Round(weatherdata.current.temp) + "°C";
            feelsLikeTemp.text = "feels like " + Mathf.Round(weatherdata.current.feels_like) + "°C";
            description.text = weatherdata.current.weather[0].description;
            precipitation.text = "";
            dayOfTheWeek.text = DateTime.Now.DayOfWeek.ToString();
            humidity.text = "Humidity: " + weatherdata.current.humidity + "%";
            wind.text = "Wind: " + Mathf.Round(weatherdata.current.wind_speed * 3.6f) + "km/h";

            //alert
            //Debug.Log("End of alert = " + new DateTime(1970, 1, 1).AddSeconds(weatherdata.alerts[0].end + weatherdata.timezone_offset).ToString("HH:mm dd.MM.yyyy"));
            if (weatherdata.alerts != null && weatherdata.alerts.Count > 0 && DateTime.Compare(DateTime.Now, new DateTime(1970, 1, 1).AddSeconds(weatherdata.alerts[0].end + weatherdata.timezone_offset)) < 0)
            {
                alertText.text = weatherdata.alerts[0].event_;
                alertText.transform.parent.gameObject.SetActive(true);
                alertPopup.SetText(weatherdata.alerts[0]);
            }
            else
            {
                alertText.transform.parent.gameObject.SetActive(false);
            }

            //sunrise/ sunset
            sunrise.text = new DateTime(1970, 1, 1).AddSeconds(weatherdata.current.sunrise + weatherdata.timezone_offset).ToString("HH:mm");
            sunset.text = new DateTime(1970, 1, 1).AddSeconds(weatherdata.current.sunset + weatherdata.timezone_offset).ToString("HH:mm");

            //time and location
            time.text = DateTime.Now.ToString("HH:mm");
            //location.text = weatherdata.name;

            UpdateCurrentWeatherIcon(weatherdata);
        }
        else
            Debug.LogError("No current weather data received");
    }

    /// <summary>
    /// Updates the current weather icon by passing weatherdata to service layer which finds fitting sprite images.
    /// </summary>
    /// <param name="weatherdata"></param>
    private void UpdateCurrentWeatherIcon(WeatherResponse weatherdata)
    {
        try
        {
            //rend.material.mainTexture = service.GetWeatherIcon(imageId);
            Sprite[] weatherSprites = service.GetWeatherIcons(weatherdata.current, weatherdata.timezone_offset, false);

            SetSprite(thunderImage, weatherSprites[0]);
            SetSprite(conditionImage, weatherSprites[1]);
            SetSprite(freezeImage, weatherSprites[2]);
            SetSprite(cloudsImage, weatherSprites[3]);
            SetSprite(sunMoonImage, weatherSprites[4]);
        }
        catch (Exception ex)
        {
            Debug.LogError("Exception at setting sprites to images: " + ex.Message + " " + ex.StackTrace);
        }
    }

    /// <summary>
    /// Sets sprite to image if sprite is not null. Deactivates GameObject otherwise.
    /// </summary>
    /// <param name="image">Image to set sprite onto.</param>
    /// <param name="sprite">Sprite to set on image.</param>
    private void SetSprite(Image image, Sprite sprite)
    {
        if (sprite != null)
        {
            image.sprite = sprite;
            image.gameObject.SetActive(true);
        }
        else
        {
            image.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Displays hourly forecast 
    /// </summary>
    /// <param name="fwd"></param>
    private void DisplayHourlyWeather(WeatherResponse weatherData)
    {
        if (hourlyForecastPrefab != null)
        {
            //Destroy all current children first
            for (int i = 0; i < hourForecastParent.childCount; i++)
            {
                Destroy(hourForecastParent.GetChild(i).gameObject);
            }

            //instantiates 24 hour forecast prefabs
            for (int i = 0; i < 26; i += 3)
            {
                //Debug.Log(new DateTime(1970, 1, 1).AddSeconds(weatherData.hourly[i].dt + weatherData.timezone_offset).ToString("HH:mm dd.MM.yyyy"));

                GameObject go = Instantiate(hourlyForecastPrefab, hourForecastParent);
                Slider[] sliders = go.GetComponentsInChildren<Slider>();
                TextMeshProUGUI[] texts = go.GetComponentsInChildren<TextMeshProUGUI>();

                int temp = (int)Mathf.Round((weatherData.hourly[i].temp + weatherData.hourly[i + 1].temp + weatherData.hourly[i + 2].temp) / 3);
                texts[0].text = temp + "";
                texts[1].text = new DateTime(1970, 1, 1).AddSeconds(weatherData.hourly[i].dt + weatherData.timezone_offset).ToString("HH:mm");

                if (temp > 0)
                {
                    sliders[0].value = temp;
                    sliders[1].value = 0;
                    texts[0].transform.localPosition = new Vector3(texts[0].transform.localPosition.x, texts[0].transform.localPosition.y + temp * 3 + 10, 0);
                }
                else
                {
                    sliders[0].value = 0;
                    sliders[1].value = temp * -1;
                    texts[0].transform.localPosition = new Vector3(texts[0].transform.localPosition.x, texts[0].transform.localPosition.y - 20 + temp * 3 + 10, 0);
                }

                //visualizes precipitation
                if (weatherData.hourly[i].rain != null || weatherData.hourly[i + 1].rain != null || weatherData.hourly[i + 2].rain != null)
                {
                    float rainInLiterPerSquaremeter = 0;
                    if (weatherData.hourly[i].rain != null)
                        rainInLiterPerSquaremeter += weatherData.hourly[i].rain.oneH;
                    if (weatherData.hourly[i + 1].rain != null)
                        rainInLiterPerSquaremeter += weatherData.hourly[i + 1].rain.oneH;
                    if (weatherData.hourly[i + 2].rain != null)
                        rainInLiterPerSquaremeter += weatherData.hourly[i + 2].rain.oneH;

                    Transform precipitationPointTransform = go.GetComponentsInChildren<Image>()[4].gameObject.transform;
                    precipitationPointTransform.localPosition = new Vector2(precipitationPointTransform.localPosition.x, rainInLiterPerSquaremeter);

                    Debug.Log(rainInLiterPerSquaremeter + "l/m²");
                }
            }
        }
        else
        {
            Debug.LogError("No hourlyForecast GameObject set, can't visualize hourly forecast.");
        }
    }

    /// <summary>
    /// Displays daily forecast data.
    /// </summary>
    /// <param name="fwd"></param>
    private void DisplayDailyForecast(WeatherResponse fwd)
    {
        if (dailyForecastPrefab != null)
        {
            //Destroy all current children first
            for (int i = 0; i < dayForecastParent.childCount; i++)
            {
                Destroy(dayForecastParent.GetChild(i).gameObject);
            }

            //instantiates new prefabs
            for (int i = 0; i < 5; i++)
            {
                Sprite[] weatherSprites = service.GetWeatherIcons(fwd.daily[i], fwd.timezone_offset, true);
                GameObject go = Instantiate(dailyForecastPrefab, dayForecastParent);
                TextMeshProUGUI[] texts = go.GetComponentsInChildren<TextMeshProUGUI>();
                Image[] images = go.GetComponentsInChildren<Image>();

                texts[0].text = i != 0 ? DateTime.Now.AddDays(i).DayOfWeek.ToString() : "Today";
                texts[1].text = Mathf.Round(fwd.daily[i].temp.min) + "°/" + Mathf.Round(fwd.daily[i].temp.max) + "°";
                texts[2].text = Mathf.Round(fwd.daily[i].pop * 100) + "%";
                //texts[2].text = Mathf.Round(fwd.daily[i].rain) + "l/m²";

                SetSprite(images[5], weatherSprites[0]);
                SetSprite(images[4], weatherSprites[1]);
                SetSprite(images[3], weatherSprites[2]);
                SetSprite(images[2], weatherSprites[3]);
                SetSprite(images[1], weatherSprites[4]);
            }
        }
        else
        {
            Debug.LogError("No daily Forecast GameObject set, can't visualize daily forecast.");
        }
    }


    /// <summary>
    /// Switches between daily and hourly forecast
    /// </summary>
    public void ShowDailyOrHourlyForecast()
    {
        dayForecastParent.gameObject.SetActive(!dayForecastParent.gameObject.activeInHierarchy);
        hourForecastParent.transform.parent.gameObject.SetActive(!hourForecastParent.transform.parent.gameObject.activeInHierarchy);
    }
}
