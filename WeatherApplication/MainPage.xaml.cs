using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using System.Collections.Generic; 
using System.Linq; 

namespace WeatherApplication
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnGetWeatherClicked(object sender, EventArgs e)
        {
            string city = cityEntry.Text?.Trim();

            if (string.IsNullOrEmpty(city))
            {
                await DisplayAlert("Error", "Please enter a city name", "OK");
                return;
            }

            await GetWeatherAndUpdateUI(city);
        }

        private async Task ChangeBackgroundColorBasedOnWeather(string weatherMain)
        {
            Color targetColor;

            switch (weatherMain)
            {
                case "Clear":
                    targetColor = Colors.LightSkyBlue;
                    break;
                case "Clouds":
                    targetColor = Colors.LightGray;
                    break;
                case "Rain":
                    targetColor = Colors.SlateGray;
                    break;
                case "Snow":
                    targetColor = Colors.LightBlue;
                    break;
                case "Thunderstorm":
                    targetColor = Colors.DarkSlateBlue;
                    break;
                case "Drizzle":
                    targetColor = Colors.SteelBlue;
                    break;
                case "Mist":
                case "Fog":
                case "Haze":
                    targetColor = Colors.LightSlateGray;
                    break;
                default:
                    targetColor = Colors.White;
                    break;
            }

            
            await this.ToColor(targetColor, 1000); 
        }

        private async Task<WeatherResponse> GetWeatherAsync(string city)
        {
            string apiKey = "5579a31aceeb763e4fdc0ff70b046512";  
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric"; 

            using (var client = new HttpClient())
            {
                var response = await client.GetStringAsync(url);
                return JsonConvert.DeserializeObject<WeatherResponse>(response);
            }
        }

        private async Task<WeatherForecastResponse> GetFiveDayForecastAsync(string city)
        {
            string apiKey = "5579a31aceeb763e4fdc0ff70b046512"; 
            string url = $"https://api.openweathermap.org/data/2.5/forecast?q={city}&appid={apiKey}&units=metric";

            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetStringAsync(url);
                return JsonConvert.DeserializeObject<WeatherForecastResponse>(response);
            }
        }
        private Dictionary<string, List<WeatherItem>> GroupForecastByDate(List<WeatherItem> items)
        {
            return items
            .GroupBy(i => DateTime.Parse(i.dt_txt).ToString("yyyy-MM-dd"))
            .ToDictionary(g => g.Key, g => g.ToList());
        }
        private async void OnRefresh(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(cityEntry.Text))
            {
                await GetWeatherAndUpdateUI(cityEntry.Text.Trim());
            }

            
            refreshView.IsRefreshing = false;
        }

        
        private async Task GetWeatherAndUpdateUI(string city)
        {
            
            temperatureLabel.Text = "Temperature: ";
            weatherLabel.Text = "Weather: ";
            humidityLabel.Text = "Humidity: ";
            windSpeedLabel.Text = "Wind Speed: ";
            pressureLabel.Text = "Pressure: ";
            weatherIcon.Source = null;

            
            loadingIndicator.IsRunning = true;
            loadingIndicator.IsVisible = true;

            try
            {
                
                var weather = await GetWeatherAsync(city);

                
                temperatureLabel.Text = $"Temperature: {weather.Main.Temp}°C";
                weatherLabel.Text = $"Weather: {weather.Weather[0].Description}";
                humidityLabel.Text = $"Humidity: {weather.Main.Humidity}%";
                windSpeedLabel.Text = $"Wind Speed: {weather.Wind.Speed} m/s";
                pressureLabel.Text = $"Pressure: {weather.Main.Pressure} hPa";

                
                string iconCode = weather.Weather[0].Icon;
                string iconUrl = $"http://openweathermap.org/img/wn/{iconCode}@4x.png";
                weatherIcon.Source = iconUrl;

                DateTime utcNow = DateTime.UtcNow;
                TimeSpan offset = TimeSpan.FromSeconds(weather.Timezone);
                DateTime localTime = utcNow + offset;
                localTimeLabel.Text = $"Local Time: {localTime:HH:mm}";

                string weatherMain = weather.Weather[0].Main;

                
                var forecast = await GetFiveDayForecastAsync(city);
                var groupedForecast = GroupForecastByDate(forecast.list);
                forecastLayout.Children.Clear(); 
                foreach (var dayForecast in groupedForecast)
                {
                    AddDailyForecast(dayForecast.Key, dayForecast.Value);
                }

                await ChangeBackgroundColorBasedOnWeather(weatherMain);


            }
            catch (Exception ex)
            {
                
                await DisplayAlert("Error", $"Failed to get weather data: {ex.Message}", "OK");
            }
            finally
            {
                
                loadingIndicator.IsRunning = false;
                loadingIndicator.IsVisible = false;
            }
        }

        private void AddDailyForecast(string date, List<WeatherItem> forecastItems)
        {
            
            Frame dayFrame = new Frame
            {
                Style = (Style)Resources["DayFrameStyle"] 
            };

            
            StackLayout dayLayout = new StackLayout();

           
            Microsoft.Maui.Controls.Label dateLabel = new Microsoft.Maui.Controls.Label
            {
                Text = DateTime.Parse(date).ToString("ddd, MMM dd"),
                Style = (Style)Resources["DateLabelStyle"] 
            };
            dayLayout.Children.Add(dateLabel);

            
            if (forecastItems.Any())
            {
                WeatherItem representativeItem = forecastItems.First(); 

                
                Image iconImage = new Image
                {
                    Source = $"http://openweathermap.org/img/wn/{representativeItem.weather[0].icon}@2x.png",
                    WidthRequest = 50,
                    HeightRequest = 50
                };
                dayLayout.Children.Add(iconImage);

                
                Microsoft.Maui.Controls.Label tempLabel = new Microsoft.Maui.Controls.Label
                {
                    Text = $"{representativeItem.main.temp:F0}°C",
                    Style = (Style)Resources["TempLabelStyle"] 
                };
                dayLayout.Children.Add(tempLabel);

                
                Microsoft.Maui.Controls.Label descriptionLabel = new Microsoft.Maui.Controls.Label
                {
                    Text = representativeItem.weather[0].description,
                    Style = (Style)Resources["DescriptionLabelStyle"] 
                };
                dayLayout.Children.Add(descriptionLabel);
            }

            dayFrame.Content = dayLayout;
            forecastLayout.Children.Add(dayFrame);
        }
    }

    
    public class WeatherResponse
    {
        public MainWeatherInfo Main { get; set; }
        public List<WeatherInfo> Weather { get; set; }
        public WindInfo Wind { get; set; }
        public int Timezone { get; set; }
    }

    public class MainWeatherInfo
    {
        public double Temp { get; set; }
        public int Humidity { get; set; }
        public int Pressure { get; set; }
    }

    public class WeatherInfo
    {
        public string Main { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
    }

    public class WindInfo
    {
        public double Speed { get; set; }
    }
    public class WeatherForecastResponse
    {
        public List<WeatherItem> list { get; set; }
    }

    public class WeatherItem
    {
        public Main main { get; set; }
        public List<Weather> weather { get; set; }
        public string dt_txt { get; set; }
    }

    public class Main
    {
        public double temp { get; set; }
    }

    public class Weather
    {
        public string description { get; set; }
        public string icon { get; set; }
    }
}