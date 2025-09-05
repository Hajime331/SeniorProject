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

        // 供「前台 Members 清單頁」使用
        // 非同步地取得全部會員清單
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

        // 呼叫 API 登入，成功回傳 MemberDto，失敗拋例外
        public async Task<MemberDto?> LoginAsync(string email, string password)
        {
            var body = new { email, password };
            var resp = await _http.PostAsJsonAsync("api/Members/login", body);

            if (resp.StatusCode == HttpStatusCode.Unauthorized)
                throw new InvalidOperationException("Email 或密碼不正確");

            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<MemberDto>();
        }


        // 供「後台 Dashboard（效率版）」使用
        // 非同步地取得會員總數
        public async Task<int> GetMemberCountAsync()
            => await _http.GetFromJsonAsync<int>("api/Members/count");

        // 非同步地取得最新的 N 筆會員清單
        public async Task<List<MemberDto>> GetRecentMembersAsync(int take = 5)
            => await _http.GetFromJsonAsync<List<MemberDto>>($"api/Members/recent?take={take}");


        // 供「會員個人資料頁」使用
        // 取得單一會員資料
        public async Task<MemberDto> GetMemberAsync(Guid id)
            => await _http.GetFromJsonAsync<MemberDto>($"api/Members/{id}");

        // 更新會員暱稱
        public async Task UpdateMemberNicknameAsync(Guid id, string nickname)
        {
            var dto = new { Nickname = nickname };

            // 呼叫 PUT /api/Members/{id}
            // 注意：這裡用 PutAsJsonAsync，因為沒有 MemberUpdateNicknameDto
            var resp = await _http.PutAsJsonAsync($"api/Members/{id}", dto);

            // 如果失敗，拋例外
            if (!resp.IsSuccessStatusCode)
            {
                // 你可以在這裡把 ProblemDetails 轉為人性化訊息
                // 這裡先簡單拋出，讓 Controller 顯示 Error
                var body = await resp.Content.ReadAsStringAsync();
                throw new ApplicationException($"更新暱稱失敗：{resp.StatusCode}\n{body}");
            }
        }


    }
}