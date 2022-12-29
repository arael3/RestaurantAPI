using RestaurantAPI.Models;

namespace RestaurantAPI.Services
{
    public interface IWeatherForecastService
    {
        IEnumerable<WeatherForecast> Get();
        IEnumerable<WeatherForecast> Get(int numberOfResults = 5, int minTempC = -20, int maxTempC = 55);
    }
}
