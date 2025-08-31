using System.Net.Http.Json;

namespace Meow.Web.Services
{
    public class BackendApi(HttpClient http) : IBackendApi
    {
        public async Task<IEnumerable<WeatherDto>> GetWeatherAsync()
        {
            // 呼叫 API 範例端點 /weatherforecast
            return await http.GetFromJsonAsync<IEnumerable<WeatherDto>>("/weatherforecast") ?? [];
        }
    }
}