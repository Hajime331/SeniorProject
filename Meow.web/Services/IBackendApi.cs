using Meow.Shared.Dtos.Accounts;
using Meow.Shared.Dtos.Common;
using Meow.Shared.Dtos.TrainingSessions;
using Meow.Web.Models;

namespace Meow.Web.Services
{
    // DTO 定義
    public record WeatherDto(DateOnly Date, int TemperatureC, string? Summary);
    public record TagDto(Guid TagID, string Name);

    public interface IBackendApi
    {
        // Weather 測試
        Task<IEnumerable<WeatherDto>> GetWeatherAsync();

        // Tag API
        Task<IEnumerable<TagDto>> GetTagsAsync();



        // 供「前台 Members 清單頁」使用
        // 抓全部會員清單
        Task<IEnumerable<MemberDto>> GetMembersAsync();



        // 供「後台 Dashboard（效率版）」使用
        // 會員總數
        Task<int> GetMemberCountAsync();

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


        // 供「會員訓練紀錄頁」使用
        // 取得會員的訓練紀錄清單（分頁、可篩選日期區間）
        Task<PagedResultDto<TrainingSessionListItemDto>> GetTrainingSessionsAsync(
        Guid memberId, DateTime? from, DateTime? to, int page, int pageSize);

        // 取得單一訓練紀錄的詳細資料
        Task<TrainingSessionDetailDto> StartTrainingSessionAsync(Guid memberId, TrainingSessionCreateDto dto);
    }
}
