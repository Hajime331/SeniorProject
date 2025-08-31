namespace Meow.Web.Services
{
    // 對應 API /weatherforecast 回傳的資料模型
    public record WeatherDto(DateOnly Date, int TemperatureC, string? Summary);

    public interface IBackendApi
    {
        Task<IEnumerable<WeatherDto>> GetWeatherAsync();
    }
}