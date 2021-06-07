using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using Assets.Scripts.Data;
using System.Collections.Generic;
using System.Collections;

public class WeatherDisplay : MonoBehaviour
{
    private WeatherService service;
    private List<Button> dailyForecastButtons = new List<Button>();
    private int currentlyActiveDayButton = 0;
    private int hourStepsOnSlider;
    private DateTime lastInteraction = DateTime.MaxValue;
    private bool startedDelayedRequest = false;
    private int timeBetweenRetries = 1;

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
    private bool executeSliderMethod = true;

    void Awake()
    {
        service = GetComponent<WeatherService>();
        sliderTimeText = hourSlider.transform.parent.GetComponentsInChildren<TextMeshProUGUI>();
        hourStepsOnSlider = Mathf.RoundToInt(24 / (hourSlider.maxValue + 1));
    }

    private void OnApplicationFocus(bool focus)
    {
        //updates 5 minutes after last interaction or after 1 minute without interaction when focus is on UI (also resets UI)
        if (focus && (DateTime.Compare(DateTime.Now, lastInteraction) > 0 && (DateTime.Now - lastInteraction).TotalMinutes >= 5) || (DateTime.Compare(DateTime.Now, lastInteraction) < 0 && DateTime.Now.Second == 0))
        {
            GetAndDisplayCurrentWeather();
            GetAndDisplayForecastWeather();
        }
    }

    //gets current weather every 10minutes and forecast weather every 60 minutes
    private void Update()
    {
        //updates 5 minutes after last interaction or after 1 minute without interaction (also resets UI)
        if ((DateTime.Compare(DateTime.Now, lastInteraction) > 0 && (DateTime.Now - lastInteraction).TotalMinutes >= 5) || (DateTime.Compare(DateTime.Now, lastInteraction) < 0 && DateTime.Now.Second == 0))
        {
            GetAndDisplayCurrentWeather();
            lastInteraction = DateTime.MaxValue;
        }
        //gets data every full hour or 5 minutes after last interaction (also resets UI)
        if ((DateTime.Compare(DateTime.Now, lastInteraction) > 0 && (DateTime.Now - lastInteraction).Minutes >= 5) || (DateTime.Compare(DateTime.Now, lastInteraction) < 0 && DateTime.Now.Minute == 0))
        {
            GetAndDisplayForecastWeather();
            lastInteraction = DateTime.MaxValue;
        }

        //retries if getting data from backend failed - only works at beginning
        if ((currentWeatherResponse == null || hourlyWeatherResponse == null || dailyWeatherResponse == null) && !startedDelayedRequest)
        {
            Debug.Log("Trying to get weather data after unsuccessfull request.");
            StartCoroutine(RequestResponsesDelayed(timeBetweenRetries));
        }
    }

    /// <summary>
    /// Method which waits t seconds after unsuccessfull request
    /// </summary>
    /// <param name="t">Time in seconds to wait after trying to get data</param>
    /// <returns></returns>
    public IEnumerator RequestResponsesDelayed(float t)
    {
        startedDelayedRequest = true;
        GetAndDisplayCurrentWeather();
        GetAndDisplayForecastWeather();
        yield return new WaitForSeconds(t);
        startedDelayedRequest = false;
    }

    private void GetAndDisplayCurrentWeather()
    {
        Debug.Log("Getting current weather data");

        //resets slider and button
        ResetSliderAndButtons(0);

        currentWeatherResponse = service.GetCurrentWeather();
        DisplayWeatherData(currentWeatherResponse.current);
        Debug.Log("Got current weather data, weatherId = " + currentWeatherResponse.current.weather[0].id);
    }

    private void GetAndDisplayForecastWeather()
    {
        Debug.Log("Getting forecast weather data");

        //resets slider and button
        ResetSliderAndButtons(0);
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

            currentTemp.text = String.Format("{0}°C", Mathf.Round(temp));
            feelsLikeTemp.text = String.Format("feels like {0}°C", Mathf.Round(feelsLike));
            description.text = weatherData.weather[0].description;

            precipitation.transform.parent.gameObject.SetActive(weatherData.dataType != DataType.Current);
            if (weatherData.dataType != DataType.Current)
            {
                precipitation.text = String.Format("{0}%", weatherData.dataType == DataType.Daily ? Mathf.RoundToInt(((DailyWeatherData)weatherData).pop * 100) : Mathf.RoundToInt(((HourlyWeatherData)weatherData).pop * 100));
            }
            DateTime date = new DateTime(1970, 1, 1).AddSeconds(weatherData.dt + weatherResponse.timezone_offset);
            dayOfTheWeek.text = date.DayOfWeek.ToString() + date.ToString(" dd.MM.yyyy");
            humidity.text = String.Format("{0}%", weatherData.humidity);
            wind.text = String.Format("{0}km/h", Mathf.Round(weatherData.wind_speed * 3.6f));

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

            //Updates slider text depending on data
            if (weatherData.dataType == DataType.Current)
            {
                UpdateSliderTimeText(true, DateTime.Now.Hour);
            }
            else if (weatherData.dataType == DataType.Daily)
            {
                if ((new DateTime(1970, 1, 1).AddSeconds(weatherData.dt + weatherResponse.timezone_offset) - DateTime.Now).TotalDays > 2)
                {
                    UpdateSliderTimeText(false);
                }
                else
                {
                    UpdateSliderTimeText(true);
                }
            }

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

                texts[0].text = i != 0 ? DateTime.Now.AddDays(i).DayOfWeek.ToString().Substring(0, 3) + "." : "Today";
                texts[1].text = Mathf.Round(dailyWeatherResponse.daily[i].d_temp.max) + "°";
                texts[2].text = Mathf.Round(dailyWeatherResponse.daily[i].d_temp.min) + "°";

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
        lastInteraction = DateTime.Now;

        if (daysInTheFuture == 0)
        {
            DisplayWeatherData(currentWeatherResponse.current);
        }
        else
        {
            DisplayWeatherData(dailyWeatherResponse.daily[daysInTheFuture]);
        }

        ResetSliderAndButtons(daysInTheFuture);
    }

    /// <summary>
    /// Resets slider handle to 0 and sets the button at the passed position to inactive
    /// </summary>
    /// <param name="highlightButton">Button to set to inactive/ highlight.</param>
    private void ResetSliderAndButtons(int highlightButton)
    {
        //activates all other buttons and deactivates itself
        for (int i = 0; i < dailyForecastButtons.Count; i++)
        {
            dailyForecastButtons[i].interactable = (i == highlightButton) ? false : true;
        }

        //sets out slider method because the value of the slider needs to be changed to represent time of loaded data
        executeSliderMethod = false;
        if (highlightButton == 0)
        {
            hourSlider.value = 0;
        }
        else
        {
            hourSlider.value = 13;
        }
    }

    /// <summary>
    /// Method which gets executed when the slider values changes.
    /// </summary>
    public void UpdateDisplayedWeatherBySlider()
    {
        if (executeSliderMethod)
        {
            lastInteraction = DateTime.Now;

            //Shows hourly weather data, except slider is on today and is 
            if (currentlyActiveDayButton == 0 && hourSlider.value == 0)
            {
                DisplayWeatherData(currentWeatherResponse.current);
            }
            else
            {
                if (currentlyActiveDayButton != 0)
                {
                    int index = currentlyActiveDayButton * 24 - DateTime.Now.Hour + Mathf.RoundToInt(hourSlider.value * hourStepsOnSlider);
                    //Count -2 because of backend bug
                    if (index < hourlyWeatherResponse.hourly.Count - 2)
                    {
                        DisplayWeatherData(hourlyWeatherResponse.hourly[index]);
                    }
                }
                else
                {
                    DisplayWeatherData(hourlyWeatherResponse.hourly[Mathf.RoundToInt(hourSlider.value * hourStepsOnSlider)]);
                }
            }
        }
        else
        {
            executeSliderMethod = true;
        }
    }

    /// <summary>
    /// Updates the slider visibility and hours to display beneath the slider
    /// </summary>
    /// <param name="displaySlider"></param>
    /// <param name="hourToStartWith"></param>
    private void UpdateSliderTimeText(bool displaySlider, int hourToStartWith = 0)
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