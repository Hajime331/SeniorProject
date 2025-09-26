using Meow.Shared.Dtos.Accounts;
using Meow.Shared.Dtos.Analytics;
using Meow.Shared.Dtos.Common;
using Meow.Shared.Dtos.Tags;
using Meow.Shared.Dtos.TrainingSessions;
using Meow.Shared.Dtos.TrainingSets;
using Meow.Shared.Dtos.Videos;
using Meow.Web.Models;
using Microsoft.AspNetCore.WebUtilities;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace Meow.Web.Services
{
    public class BackendApi : IBackendApi
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public BackendApi(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        public async Task<IEnumerable<WeatherDto>> GetWeatherAsync()
        {
            // 呼叫 API 範例端點 /weatherforecast
            return await _http.GetFromJsonAsync<IEnumerable<WeatherDto>>("/weatherforecast") ?? [];
        }

        public async Task<IReadOnlyList<TagDto>> GetTagsAsync()
        {
            return await _http.GetFromJsonAsync<List<TagDto>>("api/Tags") ?? new List<TagDto>();
        }


        public async Task<IReadOnlyList<TagDto>> GetTagsAsync(string? keyword)
        {
            var url = string.IsNullOrWhiteSpace(keyword)
                ? "api/Tags"
                : QueryHelpers.AddQueryString("api/Tags", new Dictionary<string, string?> { ["keyword"] = keyword });

            return await _http.GetFromJsonAsync<List<TagDto>>(url) ?? new List<TagDto>();
        }

        public async Task<TagDto> CreateTagAsync(TagCreateDto dto)
        {
            var resp = await _http.PostAsJsonAsync("api/Tags", dto);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                throw new ApplicationException($"CreateTag failed: {(int)resp.StatusCode} {resp.StatusCode}\n{body}");
            }
            return (await resp.Content.ReadFromJsonAsync<TagDto>())!;
        }

        public async Task UpdateTagAsync(Guid tagId, TagUpdateDto dto)
        {
            var resp = await _http.PutAsJsonAsync($"api/Tags/{tagId}", dto);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                throw new ApplicationException($"UpdateTag failed: {(int)resp.StatusCode} {resp.StatusCode}\n{body}");
            }
        }

        public async Task DeleteTagAsync(Guid tagId)
        {
            var resp = await _http.DeleteAsync($"api/Tags/{tagId}");
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                throw new ApplicationException($"DeleteTag failed: {(int)resp.StatusCode} {resp.StatusCode}\n{body}");
            }
        }


        // 供「前台 Members 清單頁」使用
        // 非同步地取得全部會員清單
        public async Task<IEnumerable<MemberDto>> GetMembersAsync()
        {
            // 相對路徑 "api/Members"，真正的主機位址稍後用 BaseAddress 設定。
            var list = await _http.GetFromJsonAsync<IEnumerable<MemberDto>>("api/Members");
            return list ?? [];
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
            var resp = await _http.PostAsJsonAsync("api/Auth/login", body);

            if (resp.StatusCode == HttpStatusCode.Unauthorized)
                throw new InvalidOperationException("Email 或密碼不正確");

            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<MemberDto>();
        }


        // 供「後台 Dashboard（效率版）」使用
        // 非同步地取得會員總數
        public async Task<int> GetMembersCountAsync()
            => await _http.GetFromJsonAsync<int>("api/Members/count");

        // 非同步地取得最新的 N 筆會員清單
        public async Task<List<MemberDto>> GetRecentMembersAsync(int take = 5)
        {
            return await _http.GetFromJsonAsync<List<MemberDto>>($"api/Members/recent?take={take}");
        }


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
                var body = await resp.Content.ReadAsStringAsync();
                // 這樣你在 Server log 或 Debug 可以直接看到 401/403/400 與 ProblemDetails
                throw new ApplicationException($"UpdateNickname failed: {(int)resp.StatusCode} {resp.StatusCode}\n{body}");
            }
        }

        public async Task ChangePasswordAsync(Guid id, ChangePasswordDto dto)
        {
            var resp = await _http.PutAsJsonAsync($"api/Members/{id}/password", dto);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                throw new ApplicationException($"ChangePassword failed: {(int)resp.StatusCode} {resp.StatusCode}\n{body}");
            }
        }


        // 新增：開始新的訓練課表
        public async Task<TrainingSessionDetailDto> StartTrainingSessionAsync(Guid memberId, TrainingSessionCreateDto dto)
        {
            var url = QueryHelpers.AddQueryString("api/TrainingSessions", new Dictionary<string, string?>
            {
                ["memberId"] = memberId.ToString()
            });

            var resp = await _http.PostAsJsonAsync(url, dto);
            resp.EnsureSuccessStatusCode();

            var result = await resp.Content.ReadFromJsonAsync<TrainingSessionDetailDto>();
            return result!;
        }


        public async Task<TrainingSessionDetailDto> CompleteTrainingSessionAsync(Guid sessionId, TrainingSessionCompleteDto dto)
        {
            var resp = await _http.PutAsJsonAsync($"api/TrainingSessions/{sessionId}/complete", dto);
            resp.EnsureSuccessStatusCode();
            return (await resp.Content.ReadFromJsonAsync<TrainingSessionDetailDto>())!;
        }


        public async Task<TrainingSessionItemDto> UpdateTrainingSessionItemAsync(TrainingSessionItemUpdateDto dto)
        {
            var resp = await _http.PutAsJsonAsync("api/TrainingSessions/items", dto);
            resp.EnsureSuccessStatusCode();
            return (await resp.Content.ReadFromJsonAsync<TrainingSessionItemDto>())!;
        }


        public async Task<AdminWeeklySummaryDto> GetAdminWeeklySummaryAsync(DateTime? startLocalDate, int take = 5)
        {
            var qs = new Dictionary<string, string?>
            {
                ["take"] = Math.Clamp(take, 1, 20).ToString(),
                // 傳「台北當地日期」字串（控制週首）
                ["start"] = startLocalDate?.ToString("yyyy-MM-dd")
            };
            var url = QueryHelpers.AddQueryString("api/Analytics/admin/weekly", qs);
            var resp = await _http.GetFromJsonAsync<AdminWeeklySummaryDto>(url);
            return resp!;
        }


        public async Task<MemberWeeklySummaryDto> GetMemberWeeklySummaryAsync(Guid memberId, DateTime? startLocalDate = null)
        {
            var qs = new Dictionary<string, string?>
            {
                ["memberId"] = memberId.ToString(),
                ["start"] = startLocalDate?.ToString("yyyy-MM-dd")
            };
            var url = QueryHelpers.AddQueryString("api/Analytics/weekly", qs);
            return (await _http.GetFromJsonAsync<MemberWeeklySummaryDto>(url))!;
        }


        // 需要：using Meow.Shared.Dtos.Common;
        public async Task<List<TrainingSessionListItemDto>> GetRecentSessionsAsync(Guid memberId, int take = 3)
        {
            var qs = new Dictionary<string, string?>
            {
                ["memberId"] = memberId.ToString(),
                ["page"] = "1",
                ["pageSize"] = Math.Clamp(take, 1, 10).ToString()
            };
            var url = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString("api/TrainingSessions", qs);

            // 換成 Shared 的 PagedResultDto<T>
            var resp = await _http.GetFromJsonAsync<Meow.Shared.Dtos.Common.PagedResultDto<TrainingSessionListItemDto>>(url);

            return resp?.Items?.ToList() ?? new();
        }



        public async Task<IReadOnlyList<PopularTrainingSetDto>> GetPopularTrainingSetsAsync(
    DateTime? start = null, DateTime? end = null, int take = 10)
        {
            var qs = new Dictionary<string, string?>
            {
                ["take"] = Math.Clamp(take, 1, 50).ToString(),
                ["start"] = start?.ToString("o"),
                ["end"] = end?.ToString("o")
            };
            var url = QueryHelpers.AddQueryString("api/Analytics/admin/popular-sets", qs);

            // 先拿原始回應，讀文字內容來看錯誤
            using var resp = await _http.GetAsync(url);
            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new ApplicationException($"popular-sets failed: {(int)resp.StatusCode} {resp.StatusCode}\n{body}");

            // 成功時再做反序列化
            return System.Text.Json.JsonSerializer.Deserialize<IReadOnlyList<PopularTrainingSetDto>>(body,
                       new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                   ?? Array.Empty<PopularTrainingSetDto>();
        }

        // 供「會員分析頁」使用
        public async Task<MemberStatsDto> GetMemberStatsAsync(Guid memberId)
        {
            return (await _http.GetFromJsonAsync<MemberStatsDto>($"api/Analytics/member/stats?memberId={memberId}"))!;
        }

        // 供「會員個人資料頁」使用
        public async Task<List<AvatarDto>> GetAvatarsAsync()
        {
            return await _http.GetFromJsonAsync<List<AvatarDto>>("api/Avatars") ?? [];
        }

        // 取得會員個人資料（含暱稱、頭像、註冊日期等）
        public async Task<MemberProfileDto?> GetMemberProfileAsync(Guid memberId)
        {
            var resp = await _http.GetAsync($"api/Members/{memberId}/profile");
            if (resp.StatusCode == HttpStatusCode.NotFound)
                return null;

            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<MemberProfileDto>();
        }

        // 更新會員個人資料（暱稱、頭像、生日、性別等）
        public async Task UpdateMemberProfileAsync(Guid memberId, MemberProfileUpdateDto dto)
        {
            var resp = await _http.PutAsJsonAsync($"api/Members/{memberId}/profile", dto);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                throw new ApplicationException($"UpdateMemberProfile failed: {(int)resp.StatusCode} {resp.StatusCode}\n{body}");
            }
        }

        // 更新會員頭像
        public async Task UpdateMemberAvatarAsync(Guid memberId, Guid avatarId)
        {
            var resp = await _http.PutAsync($"api/Members/{memberId}/avatar/{avatarId}", null);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                throw new ApplicationException($"UpdateMemberAvatar failed: {(int)resp.StatusCode} {resp.StatusCode}\n{body}");
            }
        }

        public async Task<PagedResultDto<TrainingSessionListItemDto>> GetTrainingSessionsAsync(
        Guid memberId, DateTime? from, DateTime? to, int page, int pageSize,
        IEnumerable<string>? tagIds = null)
        {
            var qs = new List<string>
        {
            $"memberId={memberId}",
            $"page={page}",
            $"pageSize={pageSize}"
        };
            if (from.HasValue) qs.Add($"from={Uri.EscapeDataString(from.Value.ToString("o"))}");
            if (to.HasValue) qs.Add($"to={Uri.EscapeDataString(to.Value.ToString("o"))}");

            if (tagIds is not null)
            {
                var tokens = tagIds.Where(s => !string.IsNullOrWhiteSpace(s))
                                   .Distinct()
                                   .Select(Uri.EscapeDataString)
                                   .ToArray();
                if (tokens.Length > 0)
                    qs.Add("tagIds=" + string.Join(",", tokens));
            }

            var url = "api/TrainingSessions?" + string.Join("&", qs);
            return await _http.GetFromJsonAsync<PagedResultDto<TrainingSessionListItemDto>>(url)
                   ?? new PagedResultDto<TrainingSessionListItemDto>
                   {
                       Items = new List<TrainingSessionListItemDto>(),
                       TotalCount = 0,
                       Page = page,
                       PageSize = pageSize
                   };
        }

        public async Task<TrainingSessionDetailDto?> GetTrainingSessionAsync(Guid sessionId)
        {
            var resp = await _http.GetAsync($"api/TrainingSessions/{sessionId}");
            if (resp.StatusCode == HttpStatusCode.NotFound) return null;
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<TrainingSessionDetailDto>();
        }


        public Task<IReadOnlyList<TrainingSetListItemDto>> GetTrainingSetsAsync(string? keyword, string? status)
           => GetTrainingSetsAsync(keyword, status, null, null);

        public async Task<IReadOnlyList<TrainingSetListItemDto>> GetTrainingSetsAsync(
            string? keyword, string? status, string? difficulty, Guid? tagId)
        {
            var qs = new Dictionary<string, string?>();
            if (!string.IsNullOrWhiteSpace(keyword)) qs["keyword"] = keyword;
            if (!string.IsNullOrWhiteSpace(status)) qs["status"] = status;
            if (!string.IsNullOrWhiteSpace(difficulty)) qs["difficulty"] = difficulty;
            if (tagId.HasValue) qs["tagId"] = tagId.ToString();

            var url = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString("api/TrainingSets", qs);
            return await _http.GetFromJsonAsync<List<TrainingSetListItemDto>>(url) ?? new List<TrainingSetListItemDto>();
        }




        // 取得影片列表 (字串 CSV)
        public async Task<IReadOnlyList<TrainingVideoListItemDto>> GetTrainingVideosAsync(string? keyword, string? status, string? tagIdsCsv)
        {
            var qs = new Dictionary<string, string?>();
            if (!string.IsNullOrWhiteSpace(keyword)) qs["keyword"] = keyword;
            if (!string.IsNullOrWhiteSpace(status)) qs["status"] = status;
            if (!string.IsNullOrWhiteSpace(tagIdsCsv)) qs["tagIds"] = tagIdsCsv;

            var url = QueryHelpers.AddQueryString("api/TrainingVideos", qs);
            return await _http.GetFromJsonAsync<List<TrainingVideoListItemDto>>(url) ?? new List<TrainingVideoListItemDto>();
        }


        // 多載：吃 IEnumerable<Guid> 自動轉 CSV
        public Task<IReadOnlyList<TrainingVideoListItemDto>> GetTrainingVideosAsync(string? keyword, string? status, IEnumerable<Guid>? tagIds)
        {
            string? csv = (tagIds != null) ? string.Join(",", tagIds) : null;
            return GetTrainingVideosAsync(keyword, status, csv);
        }


        public async Task UpdateTrainingVideoStatusAsync(Guid videoId, string status)
        {
            if (videoId == Guid.Empty) throw new ArgumentException("videoId is required", nameof(videoId));
            if (string.IsNullOrWhiteSpace(status)) throw new ArgumentException("status is required", nameof(status));

            var dto = new TrainingVideoStatusDto(status);
            var resp = await _http.PutAsJsonAsync($"api/TrainingVideos/{videoId}/status", dto);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                throw new ApplicationException($"Update video status failed: {(int)resp.StatusCode} {resp.StatusCode}\n{body}");
            }
        }


        public Task<TrainingVideoDetailDto?> GetTrainingVideoAsync(Guid id)
            => _http.GetFromJsonAsync<TrainingVideoDetailDto>($"api/TrainingVideos/{id}");



        public async Task<TrainingVideoDetailDto> CreateTrainingVideoAsync(TrainingVideoCreateDto dto)
        {
            var resp = await _http.PostAsJsonAsync("api/TrainingVideos", dto);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                throw new ApplicationException($"CreateTrainingVideo failed: {(int)resp.StatusCode} {resp.StatusCode}\n{body}");
            }
            return (await resp.Content.ReadFromJsonAsync<TrainingVideoDetailDto>())!;
        }


        public async Task<TrainingVideoDetailDto> UpdateTrainingVideoAsync(TrainingVideoUpdateDto dto)
        {
            var resp = await _http.PutAsJsonAsync($"api/TrainingVideos/{dto.VideoId}", dto);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                throw new ApplicationException($"UpdateTrainingVideo failed: {(int)resp.StatusCode} {resp.StatusCode}\n{body}");
            }
            if (resp.Content.Headers.ContentLength.GetValueOrDefault() > 0)
            {
                var updated = await resp.Content.ReadFromJsonAsync<TrainingVideoDetailDto>();
                if (updated != null) return updated;
            }
            return (await _http.GetFromJsonAsync<TrainingVideoDetailDto>($"api/TrainingVideos/{dto.VideoId}"))!;
        }


        public Task<TrainingSetDetailDto?> GetTrainingSetAsync(Guid id)
            => _http.GetFromJsonAsync<TrainingSetDetailDto>($"api/TrainingSets/{id}");



        public async Task<TrainingSetDetailDto> CreateTrainingSetAsync(TrainingSetCreateDto dto)
        {
            var resp = await _http.PostAsJsonAsync("api/TrainingSets", dto);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                throw new ApplicationException($"CreateTrainingSet failed: {(int)resp.StatusCode} {resp.StatusCode}\n{body}");
            }
            return (await resp.Content.ReadFromJsonAsync<TrainingSetDetailDto>())!;
        }


        public async Task UpdateTrainingVideoTagsAsync(Guid id, IEnumerable<Guid> tagIds)
        {
            var resp = await _http.PutAsJsonAsync($"api/TrainingVideos/{id}/tags", tagIds);
            resp.EnsureSuccessStatusCode();
        }


        public async Task<TrainingSetDetailDto> UpdateTrainingSetAsync(Guid id, TrainingSetUpdateDto dto)
        {
            var resp = await _http.PutAsJsonAsync($"api/TrainingSets/{id}", dto);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                throw new ApplicationException($"UpdateTrainingSet failed: {(int)resp.StatusCode} {resp.StatusCode}\n{body}");
            }
            // API 可能回 200 帶 body，或 204 無 body；兩者都支援
            if (resp.Content.Headers.ContentLength.GetValueOrDefault() > 0)
            {
                var updated = await resp.Content.ReadFromJsonAsync<TrainingSetDetailDto>();
                if (updated != null) return updated;
            }
            // 204 或空 body -> 再 GET 一次
            return (await _http.GetFromJsonAsync<TrainingSetDetailDto>($"api/TrainingSets/{id}"))!;
        }

        // 便利多載：沿用 dto.SetId
        public Task<TrainingSetDetailDto> UpdateTrainingSetAsync(TrainingSetUpdateDto dto)
            => UpdateTrainingSetAsync(dto.SetId, dto);


        public async Task DeleteTrainingSetAsync(Guid id)
        {
            var resp = await _http.DeleteAsync($"api/TrainingSets/{id}");
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                throw new ApplicationException($"刪除失敗：{resp.StatusCode} {body}");
            }
        }

        public async Task DeleteTrainingVideoAsync(Guid id)
        {
            var resp = await _http.DeleteAsync($"api/TrainingVideos/{id}");
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                throw new ApplicationException($"DeleteTrainingVideo failed: {(int)resp.StatusCode} {resp.StatusCode}\n{body}");
            }
        }


        public async Task<string?> UploadTrainingSetCoverAsync(Guid setId, IFormFile file)
        {
            using var content = new MultipartFormDataContent();
            await using var stream = file.OpenReadStream();
            var sc = new StreamContent(stream);
            sc.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(sc, "file", file.FileName);

            var resp = await _http.PostAsync($"api/TrainingSets/{setId}/cover", content);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode) throw new ApplicationException(body);

            var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(body);
            return dict != null && dict.TryGetValue("coverUrl", out var url) ? url : null;
        }


    }
}