using Meow.Web.Models;
using System.Net;
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

        // 呼叫 API 建立會員
        public async Task<MemberDto?> CreateMemberAsync(MemberCreateRequest req)
        {
            var resp = await _http.PostAsJsonAsync("api/Members", req);

            if (resp.StatusCode == HttpStatusCode.Conflict)
            {
                // 409：Email 已被使用，丟一個可讀訊息的例外，等會在 Controller 把它變成表單錯誤。
                throw new InvalidOperationException("Email 已被使用");
            }

            // 201/200 都會通過，其他會丟例外
            resp.EnsureSuccessStatusCode(); 
            var created = await resp.Content.ReadFromJsonAsync<MemberDto>();
            return created;
        }
    }
}