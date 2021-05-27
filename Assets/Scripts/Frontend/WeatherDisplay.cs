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
    }

    private void Start()
    {
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            GetAndDisplayCurrentWeather();
            GetAndDisplayForecastWeather();
            //GetAndDisplayCurrentDayHourlyWeather();
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
            float temp = (weatherData.dataType == DataType.Daily) ? ((DailyWeatherData)weatherData).temp.day : ((weatherData.dataType == DataType.Current) ? ((CurrentWeatherData)weatherData).temp : ((HourlyWeatherData)weatherData).temp);
            float feelsLike = (weatherData.dataType == DataType.Daily) ? ((DailyWeatherData)weatherData).feels_like.day : ((weatherData.dataType == DataType.Current) ? ((CurrentWeatherData)weatherData).feels_like : ((HourlyWeatherData)weatherData).feels_like);

            currentTemp.text = Mathf.Round(temp) + "°C";
            feelsLikeTemp.text = "feels like " + Mathf.Round(feelsLike) + "°C";

            description.text = weatherData.weather[0].description;
            precipitation.text = "";
            dayOfTheWeek.text = new DateTime(1970, 1, 1).AddSeconds(weatherData.dt + weatherResponse.timezone_offset).DayOfWeek.ToString();
            humidity.text = "Humidity: " + weatherData.humidity + "%";
            wind.text = "Wind: " + Mathf.Round(weatherData.wind_speed * 3.6f) + "km/h";

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
            Debug.LogError("No hourly weather data received");

        /*
        //instantiates 24 hour forecast prefabs
        for (int i = 0; i < 26; i += 3)
        {
            //Debug.Log(new DateTime(1970, 1, 1).AddSeconds(weatherData.hourly[i].dt + weatherData.timezone_offset).ToString("HH:mm dd.MM.yyyy"));

            Slider[] sliders = GetComponentsInChildren<Slider>();
            TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();

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

                Transform precipitationPointTransform = GetComponentsInChildren<Image>()[4].gameObject.transform;
                precipitationPointTransform.localPosition = new Vector2(precipitationPointTransform.localPosition.x, rainInLiterPerSquaremeter);
            }
        }
    }
    else
    {
        Debug.LogError("No hourlyForecast GameObject set, can't visualize hourly forecast.");
    }*/
    }

    /// <summary>
    /// Displays daily forecast data.
    /// </summary>
    private void DisplayDailyForecast()
    {
        if (dailyForecastPrefab != null)
        {
            //checks if dayForecastParent has any children, if not --> instantiates them
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
                texts[1].text = Mathf.Round(dailyWeatherResponse.daily[i].temp.min) + "°/" + Mathf.Round(dailyWeatherResponse.daily[i].temp.max) + "°";
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
            Debug.LogError("No daily Forecast GameObject set, can't visualize daily forecast.");
        }
    }

    /// <summary>
    /// Updates the displayed weather forecast information on the upper part of the GUI when a forecast prefab has been pressed
    /// </summary>
    /// <param name="daysInTheFuture">Which day in the future the displayed weather information should be from.</param>
    private void UpdateDisplayedWeatherByButton(int daysInTheFuture)
    {

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
    }

    public void UpdateDisplayedWeatherBySlider()
    {

    }

    private void UpdateSliderTimeText(bool includeSlider, int hourToStartWith)
    {
        hourSlider.transform.parent.gameObject.SetActive(includeSlider);
        Debug.Log(hourToStartWith + "");

        if (includeSlider)
        {
            for (int i = 0; i < sliderTimeText.Length; i++)
            {
                sliderTimeText[i].text = (hourToStartWith + 3*i) % 24 + ":00";
            }
        }
    }
}