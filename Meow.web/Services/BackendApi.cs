using Meow.Web.Models;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace Meow.Web.Services
{
    public class BackendApi : IBackendApi
    {
        private readonly HttpClient _http;
        public BackendApi(HttpClient http) => _http = http;

        public async Task<IEnumerable<WeatherDto>> GetWeatherAsync()
        {
            // 呼叫 API 範例端點 /weatherforecast
            return await _http.GetFromJsonAsync<IEnumerable<WeatherDto>>("/weatherforecast") ?? [];
        }

        public async Task<IEnumerable<TagDto>> GetTagsAsync()
        {
            return await _http.GetFromJsonAsync<IEnumerable<TagDto>>("/api/Tags") ?? [];
        }

        public async Task<IEnumerable<MemberDto>> GetMembersAsync()
        {
            // 相對路徑 "api/Members"，真正的主機位址稍後用 BaseAddress 設定。
            var list = await _http.GetFromJsonAsync<IEnumerable<MemberDto>>("api/Members");
            return list ?? Enumerable.Empty<MemberDto>();
        }
    }
}