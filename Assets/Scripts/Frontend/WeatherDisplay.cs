using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using Assets.Scripts.Data;
using System.Collections.Generic;

public class WeatherDisplay : MonoBehaviour
{
    private WeatherService service;
    private const int minutesBetweenCurrentWeatherRequests = 1;
    private const int minutesBetweenForecastWeatherRequests = 60;
    private float currentWeatherUpdateTime;
    private float forecastWeatherUpdateTime;
    private List<Button> dailyForecastButtons = new List<Button>();
    private int currentlyActiveDayButton = 0;
    private int hourStepsOnSlider;

    //weatherresponses
    private WeatherResponse currentWeatherResponse;
    private WeatherResponse hourlyWeatherResponse;
    private WeatherResponse dailyWeatherResponse;

    [Header("Alert")]
    public AlertPopup alertPopup;
    public GameObject alertPanel;

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
    public TextMeshProUGUI time;
    public TextMeshProUGUI location;
    public TextMeshProUGUI dayOfTheWeek;

    [Header("Forecast Prefabs")]
    public Transform dayForecastParent;
    public GameObject dailyForecastPrefab;

    [Header("Hour Slider")]
    public Slider hourSlider;
    private TextMeshProUGUI[] sliderTimeText;

    void Awake()
    {
        service = GetComponent<WeatherService>();
        sliderTimeText = hourSlider.transform.parent.GetComponentsInChildren<TextMeshProUGUI>();
        hourStepsOnSlider = Mathf.RoundToInt(24 / (hourSlider.maxValue + 1));
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            GetAndDisplayCurrentWeather();
            GetAndDisplayForecastWeather();
        }
    }

    //gets current weather every 10minutes and forecast weather every 60 minutes
    private void Update()
    {
        currentWeatherUpdateTime += Time.deltaTime;
        forecastWeatherUpdateTime += Time.deltaTime;

        //updates after 5 minutes if slider has been moved or after 1 minute if slider has not been moved
        if ((currentWeatherUpdateTime > (minutesBetweenCurrentWeatherRequests * 60) && hourSlider.value != 0) || currentWeatherUpdateTime > (minutesBetweenCurrentWeatherRequests * 300))
        {
            //reset slider if neccessary
            if (hourSlider.value != 0)
            {
                hourSlider.value = 0;
            }

            GetAndDisplayCurrentWeather();
        }
        if (forecastWeatherUpdateTime > (minutesBetweenForecastWeatherRequests * 60))
        {
            GetAndDisplayForecastWeather();
        }

        //retries if getting data from backend failed - only works at beginning
        if (currentWeatherUpdateTime > 1 && currentWeatherResponse == null)
        {
            Debug.Log("Trying to get current weather data again.");
            GetAndDisplayCurrentWeather();
        }
        if (forecastWeatherUpdateTime > 1 && (hourlyWeatherResponse == null || dailyWeatherResponse == null))
        {
            Debug.Log("Trying to get forecast weather data again.");
            GetAndDisplayForecastWeather();
        }
    }

    private void GetAndDisplayCurrentWeather()
    {
        Debug.Log("Getting current weather data");
        currentWeatherUpdateTime = 0;
        currentWeatherResponse = service.GetCurrentWeather();
        DisplayWeatherData(currentWeatherResponse.current);
        Debug.Log("Got current weather data, weatherId = " + currentWeatherResponse.current.weather[0].id);
    }

    private void GetAndDisplayForecastWeather()
    {
        Debug.Log("Getting forecast weather data");
        forecastWeatherUpdateTime = 0;

        //daily forecast weather
        dailyWeatherResponse = service.GetDailyForecastWeather();
        //of next 48 hours
        hourlyWeatherResponse = service.GetWeatherOfNext48Hours();

        if (dailyWeatherResponse != null)
            DisplayDailyForecast();
        else
            Debug.LogError("No daily forecast weather data received");
    }

    /// <summary>
    /// Updates the current weather icon by passing weatherdata to service layer which finds fitting sprite images.
    /// </summary>
    /// <param name="weatherData"></param>
    private void UpdateBigWeatherIcon(WeatherData weatherData)
    {
        try
        {
            //rend.material.mainTexture = service.GetWeatherIcon(imageId);
            Sprite[] weatherSprites = service.GetWeatherIcons(weatherData);

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
    /// Displays weather information at the top part of the screen
    /// </summary>
    /// <param name="weatherData">Datat to display</param>
    private void DisplayWeatherData(WeatherData weatherData)
    {
        if (weatherData != null)
        {
            //gets correct data depending on datatype
            WeatherResponse weatherResponse = (weatherData.dataType == DataType.Daily) ? dailyWeatherResponse : ((weatherData.dataType == DataType.Current) ? currentWeatherResponse : hourlyWeatherResponse);
            float temp = (weatherData.dataType == DataType.Daily) ? weatherData.d_temp.day : weatherData.temp;
            float feelsLike = (weatherData.dataType == DataType.Daily) ? weatherData.d_feels_like.day : weatherData.feels_like;

            currentTemp.text = Mathf.Round(temp) + "°C";
            feelsLikeTemp.text = "feels like " + Mathf.Round(feelsLike) + "°C";

            description.text = weatherData.weather[0].description;

            precipitation.gameObject.SetActive(weatherData.dataType != DataType.Current);
            if (weatherData.dataType != DataType.Current)
            {
                precipitation.text = String.Format("Precipitation: {0}%", weatherData.dataType == DataType.Daily ? Mathf.RoundToInt(((DailyWeatherData)weatherData).pop * 100) : Mathf.RoundToInt(((HourlyWeatherData)weatherData).pop * 100));
            }

            dayOfTheWeek.text = new DateTime(1970, 1, 1).AddSeconds(weatherData.dt + weatherResponse.timezone_offset).DayOfWeek.ToString();
            humidity.text = String.Format("Humidity: {0}%", weatherData.humidity);
            wind.text = String.Format("Wind: {0}km/h", Mathf.Round(weatherData.wind_speed * 3.6f));

            //alert
            //Debug.Log("End of alert = " + new DateTime(1970, 1, 1).AddSeconds(weatherdata.alerts[0].end + weatherdata.timezone_offset).ToString("HH:mm dd.MM.yyyy"));
            if (weatherResponse.alerts != null && weatherResponse.alerts.Count > 0 && DateTime.Compare(new DateTime(1970, 1, 1).AddSeconds(weatherData.dt + weatherResponse.timezone_offset), new DateTime(1970, 1, 1).AddSeconds(weatherResponse.alerts[0].end + weatherResponse.timezone_offset)) < 0)
            {
                //alertText.text = weatherdata.alerts[0].event_;
                alertPanel.SetActive(true);
                alertPopup.SetText(weatherResponse.alerts[0]);
            }
            else
            {
                alertPanel.SetActive(false);
            }

            //time
            time.text = new DateTime(1970, 1, 1).AddSeconds(weatherData.dt + weatherResponse.timezone_offset).ToString("HH:mm");
            UpdateBigWeatherIcon(weatherData);
        }
        else
        {
            Debug.LogError("No hourly weather data received");
        }
    }

    /// <summary>
    /// Displays daily forecast data.
    /// </summary>
    private void DisplayDailyForecast()
    {
        if (dailyForecastPrefab != null)
        {
            //checks if dayForecastParent has any children, if not --> instantiate them
            if (dayForecastParent.childCount == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    GameObject go = Instantiate(dailyForecastPrefab, dayForecastParent);
                    int nr = i;
                    Button b = go.GetComponent<Button>();
                    dailyForecastButtons.Add(b);
                    b.onClick.AddListener(delegate { UpdateDisplayedWeatherByButton(nr); });
                }
                //deactivates current day button
                dailyForecastButtons[0].interactable = false;
                UpdateSliderTimeText(true, DateTime.Now.Hour);
            }

            //changes prefab values
            for (int i = 0; i < 5; i++)
            {
                Sprite[] weatherSprites = service.GetWeatherIcons(dailyWeatherResponse.daily[i]);
                GameObject forecastOfDay = dayForecastParent.GetChild(i).gameObject;
                TextMeshProUGUI[] texts = forecastOfDay.GetComponentsInChildren<TextMeshProUGUI>(true);
                Image[] images = forecastOfDay.GetComponentsInChildren<Image>(true);

                texts[0].text = i != 0 ? DateTime.Now.AddDays(i).DayOfWeek.ToString() : "Today";
                texts[1].text = Mathf.Round(dailyWeatherResponse.daily[i].d_temp.min) + "°/" + Mathf.Round(dailyWeatherResponse.daily[i].d_temp.max) + "°";
                texts[2].text = Mathf.Round(dailyWeatherResponse.daily[i].pop * 100) + "%";
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
            Debug.LogError("No daily forecast GameObject set, can't visualize daily forecast.");
        }
    }

    /// <summary>
    /// Updates the displayed weather forecast information on the upper part of the GUI when a forecast prefab has been pressed
    /// </summary>
    /// <param name="daysInTheFuture">Which day in the future the displayed weather information should be from.</param>
    private void UpdateDisplayedWeatherByButton(int daysInTheFuture)
    {
        currentlyActiveDayButton = daysInTheFuture;
        if (daysInTheFuture == 0)
        {
            DisplayWeatherData(currentWeatherResponse.current);
            UpdateSliderTimeText(true, DateTime.Now.Hour);
        }
        else if (daysInTheFuture == 1 || daysInTheFuture == 2)
        {
            DisplayWeatherData(dailyWeatherResponse.daily[daysInTheFuture]);
            UpdateSliderTimeText(true, 0);
        }
        else
        {
            DisplayWeatherData(dailyWeatherResponse.daily[daysInTheFuture]);
            UpdateSliderTimeText(false, -1);
        }

        //activates all other buttons and deactivates itself
        for (int i = 0; i < dailyForecastButtons.Count; i++)
        {
            dailyForecastButtons[i].interactable = (i == daysInTheFuture) ? false : true;
        }

        //resets slider
        hourSlider.value = 0;
    }

    /// <summary>
    /// Method which gets executed when the slider values changes.
    /// </summary>
    public void UpdateDisplayedWeatherBySlider()
    {
        //Shows hourly weather data, except slider is on today and is 
        if (currentlyActiveDayButton == 0 && hourSlider.value == 0)
        {
            DisplayWeatherData(currentWeatherResponse.current);
        }
        else
        {
            if (currentlyActiveDayButton != 0)
                DisplayWeatherData(hourlyWeatherResponse.hourly[currentlyActiveDayButton * 24 - DateTime.Now.Hour + Mathf.RoundToInt(hourSlider.value * hourStepsOnSlider)]);
            else
                DisplayWeatherData(hourlyWeatherResponse.hourly[Mathf.RoundToInt(hourSlider.value * hourStepsOnSlider)]);
        }
    }

    /// <summary>
    /// Updates the slider visibility and hours to display beneath the slider
    /// </summary>
    /// <param name="displaySlider"></param>
    /// <param name="hourToStartWith"></param>
    private void UpdateSliderTimeText(bool displaySlider, int hourToStartWith)
    {
        hourSlider.transform.parent.gameObject.SetActive(displaySlider);

        if (displaySlider)
        {
            for (int i = 0; i < sliderTimeText.Length; i++)
            {
                sliderTimeText[i].text = (hourToStartWith + 3 * i) % 24 + ":00";
            }
        }
    }
}