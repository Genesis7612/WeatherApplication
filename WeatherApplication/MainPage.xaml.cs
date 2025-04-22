using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using System.Collections.Generic; // Required for List and Dictionary
using System.Linq; // Required for GroupBy and ToDictionary

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

            // Animating background color change
            await this.ToColor(targetColor, 1000); // 1000ms for the transition
        }

        private async Task<WeatherResponse> GetWeatherAsync(string city)
        {
            string apiKey = "5579a31aceeb763e4fdc0ff70b046512";  // Use your actual API key here
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric"; // Use metric units for Celsius

            using (var client = new HttpClient())
            {
                var response = await client.GetStringAsync(url);
                return JsonConvert.DeserializeObject<WeatherResponse>(response);
            }
        }

        private async Task<WeatherForecastResponse> GetFiveDayForecastAsync(string city)
        {
            string apiKey = "5579a31aceeb763e4fdc0ff70b046512"; // ✅ Your actual API key
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

            // Stop the refresh animation
            refreshView.IsRefreshing = false;
        }

        // Helper method to refresh the UI with new weather data
        private async Task GetWeatherAndUpdateUI(string city)
        {
            // Clear old weather info
            temperatureLabel.Text = "Temperature: ";
            weatherLabel.Text = "Weather: ";
            humidityLabel.Text = "Humidity: ";
            windSpeedLabel.Text = "Wind Speed: ";
            pressureLabel.Text = "Pressure: ";
            weatherIcon.Source = null;

            // Show loading indicator while fetching weather
            loadingIndicator.IsRunning = true;
            loadingIndicator.IsVisible = true;

            try
            {
                // Fetch the weather data from the OpenWeatherMap API
                var weather = await GetWeatherAsync(city);

                // Update the labels with the new weather information
                temperatureLabel.Text = $"Temperature: {weather.Main.Temp}°C";
                weatherLabel.Text = $"Weather: {weather.Weather[0].Description}";
                humidityLabel.Text = $"Humidity: {weather.Main.Humidity}%";
                windSpeedLabel.Text = $"Wind Speed: {weather.Wind.Speed} m/s";
                pressureLabel.Text = $"Pressure: {weather.Main.Pressure} hPa";

                // Get the weather icon URL and set the Image source
                string iconCode = weather.Weather[0].Icon;
                string iconUrl = $"http://openweathermap.org/img/wn/{iconCode}@4x.png";
                weatherIcon.Source = iconUrl;

                DateTime utcNow = DateTime.UtcNow;
                TimeSpan offset = TimeSpan.FromSeconds(weather.Timezone);
                DateTime localTime = utcNow + offset;
                localTimeLabel.Text = $"Local Time: {localTime:HH:mm}";

                string weatherMain = weather.Weather[0].Main;

                // Fetch and display forecast
                var forecast = await GetFiveDayForecastAsync(city);
                var groupedForecast = GroupForecastByDate(forecast.list);
                forecastLayout.Children.Clear(); // Clear any previous forecast data
                foreach (var dayForecast in groupedForecast)
                {
                    AddDailyForecast(dayForecast.Key, dayForecast.Value);
                }

                await ChangeBackgroundColorBasedOnWeather(weatherMain);


            }
            catch (Exception ex)
            {
                // Handle errors (e.g., no internet connection, invalid city name)
                await DisplayAlert("Error", $"Failed to get weather data: {ex.Message}", "OK");
            }
            finally
            {
                // Hide the loading indicator when done
                loadingIndicator.IsRunning = false;
                loadingIndicator.IsVisible = false;
            }
        }

        private void AddDailyForecast(string date, List<WeatherItem> forecastItems)
        {
            // Create a frame for each day
            Frame dayFrame = new Frame
            {
                Style = (Style)Resources["DayFrameStyle"] // Assuming you'll define this style
            };

            // Create a stack layout for the day's forecast
            StackLayout dayLayout = new StackLayout();

            // Add the date
            Microsoft.Maui.Controls.Label dateLabel = new Microsoft.Maui.Controls.Label
            {
                Text = DateTime.Parse(date).ToString("ddd, MMM dd"),
                Style = (Style)Resources["DateLabelStyle"] // Assuming you'll define this style
            };
            dayLayout.Children.Add(dateLabel);

            // Display a simplified forecast (e.g., one item per day, or the first one)
            if (forecastItems.Any())
            {
                WeatherItem representativeItem = forecastItems.First(); // Or logic to pick a representative time

                // Icon
                Image iconImage = new Image
                {
                    Source = $"http://openweathermap.org/img/wn/{representativeItem.weather[0].icon}@2x.png",
                    WidthRequest = 50,
                    HeightRequest = 50
                };
                dayLayout.Children.Add(iconImage);

                // Temperature
                Microsoft.Maui.Controls.Label tempLabel = new Microsoft.Maui.Controls.Label
                {
                    Text = $"{representativeItem.main.temp:F0}°C",
                    Style = (Style)Resources["TempLabelStyle"] // Assuming you'll define this style
                };
                dayLayout.Children.Add(tempLabel);

                // Description
                Microsoft.Maui.Controls.Label descriptionLabel = new Microsoft.Maui.Controls.Label
                {
                    Text = representativeItem.weather[0].description,
                    Style = (Style)Resources["DescriptionLabelStyle"] // Assuming you'll define this style
                };
                dayLayout.Children.Add(descriptionLabel);
            }

            dayFrame.Content = dayLayout;
            forecastLayout.Children.Add(dayFrame);
        }
    }

    // Model for the weather API response
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