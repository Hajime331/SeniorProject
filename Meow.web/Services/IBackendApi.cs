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

        // Member API
        Task<IEnumerable<MemberDto>> GetMembersAsync();

        // 建立會員
        Task<MemberDto?> CreateMemberAsync(MemberCreateRequest req);
    }
}
