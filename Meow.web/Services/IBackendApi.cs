using Meow.Shared.Dtos.Accounts;
using Meow.Shared.Dtos.Analytics;
using Meow.Shared.Dtos.Common;
using Meow.Shared.Dtos.Tags;
using Meow.Shared.Dtos.TrainingSessions;
using Meow.Shared.Dtos.TrainingSets;
using Meow.Shared.Dtos.Videos;
using Meow.Web.Models;
using static Meow.Web.Services.BackendApi;

namespace Meow.Web.Services
{
    // DTO 定義
    public record WeatherDto(DateOnly Date, int TemperatureC, string? Summary);

    public interface IBackendApi
    {
        // Weather 測試
        Task<IEnumerable<WeatherDto>> GetWeatherAsync();

        // Tag API
        Task<IReadOnlyList<TagDto>> GetTagsAsync();

        // 新增：支援 keyword 的搜尋
        Task<IReadOnlyList<TagDto>> GetTagsAsync(string? keyword);

        // 新增：建立 / 更新 / 刪除
        Task<TagDto> CreateTagAsync(TagCreateDto dto);


        Task UpdateTagAsync(Guid tagId, TagUpdateDto dto);


        Task DeleteTagAsync(Guid tagId);


        // 供「前台 Members 清單頁」使用
        // 抓全部會員清單
        Task<IEnumerable<MemberDto>> GetMembersAsync();



        // 供「後台 Dashboard（效率版）」使用
        // 會員總數
        Task<int> GetMembersCountAsync();

        // 近期會員（依建立時間排序，預設取 5 筆）
        Task<List<MemberDto>> GetRecentMembersAsync(int take = 5);


        // 建立會員
        Task<MemberDto?> CreateMemberAsync(MemberCreateRequest req);

        // 新增：登入（呼叫 API 的 /api/Members/login）
        Task<MemberDto?> LoginAsync(string email, string password);



        // 供「會員個人資料頁」使用
        // 取得單一會員資料
        Task<MemberDto> GetMemberAsync(Guid id);

        // 更新會員暱稱
        Task UpdateMemberNicknameAsync(Guid id, string nickname);

        // 更新會員密碼
        Task ChangePasswordAsync(Guid id, ChangePasswordDto dto);


        /*// 供「會員訓練紀錄頁」使用
        // 取得會員的訓練紀錄清單（分頁、可篩選日期區間）
        Task<PagedResultDto<TrainingSessionListItemDto>> GetTrainingSessionsAsync(
        Guid memberId, DateTime? from, DateTime? to, int page, int pageSize);*/

        // 開始新的訓練紀錄
        Task<TrainingSessionDetailDto> StartTrainingSessionAsync(Guid memberId, TrainingSessionCreateDto dto);

        // 完成訓練紀錄（包含訓練項目）
        Task<TrainingSessionDetailDto> CompleteTrainingSessionAsync(Guid sessionId, TrainingSessionCompleteDto dto);

        // 更新訓練紀錄的訓練項目
        Task<TrainingSessionItemDto> UpdateTrainingSessionItemAsync(TrainingSessionItemUpdateDto dto);


        Task<AdminWeeklySummaryDto> GetAdminWeeklySummaryAsync(DateTime? startLocalDate, int take = 5);


        Task<MemberWeeklySummaryDto> GetMemberWeeklySummaryAsync(Guid memberId, DateTime? startLocalDate = null);


        Task<List<TrainingSessionListItemDto>> GetRecentSessionsAsync(Guid memberId, int take = 3);


        // 取得熱門訓練組合排行
        Task<IReadOnlyList<PopularTrainingSetDto>> GetPopularTrainingSetsAsync(
        DateTime? start = null, DateTime? end = null, int take = 10);


        Task<MemberStatsDto> GetMemberStatsAsync(Guid memberId);


        // 取得所有系統頭像
        Task<List<AvatarDto>> GetAvatarsAsync();

        // 取得指定會員的擴展資料
        Task<MemberProfileDto?> GetMemberProfileAsync(Guid memberId);

        // 更新指定會員的擴展資料（生日、性別、身高、體重）
        Task UpdateMemberProfileAsync(Guid memberId, MemberProfileUpdateDto dto);

        // 設定指定會員的頭像
        Task UpdateMemberAvatarAsync(Guid memberId, Guid avatarId);

        // 供「會員訓練紀錄頁」使用 - 進階版
        Task<PagedResultDto<TrainingSessionListItemDto>> GetTrainingSessionsAsync(
        Guid memberId, DateTime? from, DateTime? to, int page, int pageSize,
        IEnumerable<string>? tagIds = null);

        // 修正可 Null：和 BackendApi 實作一致，避免警告/錯誤
        Task<TrainingSessionDetailDto?> GetTrainingSessionAsync(Guid sessionId);


        // 影片查詢（支援 keyword/status/tagIds，多選 tagIds 以逗號字串）
        Task<IReadOnlyList<TrainingVideoListItemDto>> GetTrainingVideosAsync(string? keyword, string? status, string? tagIdsCsv);


        // 新增這個多載（吃 List<Guid>）
        Task<IReadOnlyList<TrainingVideoListItemDto>> GetTrainingVideosAsync(
            string? keyword, string? status, IEnumerable<Guid>? tagIds);


        Task UpdateTrainingVideoTagsAsync(Guid id, IEnumerable<Guid> tagIds);


        // 取得單一影片詳情
        Task<TrainingVideoDetailDto?> GetTrainingVideoAsync(Guid id);


        // 建立影片
        Task<TrainingVideoDetailDto> CreateTrainingVideoAsync(TrainingVideoCreateDto dto);



        // 影片更新
        Task<TrainingVideoDetailDto> UpdateTrainingVideoAsync(TrainingVideoUpdateDto dto);



        // 更新影片（用於未來擴充）
        Task UpdateTrainingVideoStatusAsync(Guid id, string status);


        // 刪除影片
        Task DeleteTrainingVideoAsync(Guid id);


        // 取得所有課表清單（支援 keyword/status 篩選）
        Task<IReadOnlyList<TrainingSetListItemDto>> GetTrainingSetsAsync(string? keyword, string? status);



        // 取得單一課表詳情
        Task<TrainingSetDetailDto?> GetTrainingSetAsync(Guid id);


        // 建立課表
        Task<TrainingSetDetailDto> CreateTrainingSetAsync(TrainingSetCreateDto dto);


        // 課表更新
        Task<TrainingSetDetailDto> UpdateTrainingSetAsync(Guid id, TrainingSetUpdateDto dto);

        // 刪除課表
        Task DeleteTrainingSetAsync(Guid id);



        // 便利多載
        Task<TrainingSetDetailDto> UpdateTrainingSetAsync(TrainingSetUpdateDto dto); 

    }
}
