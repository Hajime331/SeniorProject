namespace Meow.Shared.Dtos.Accounts;

// 會員擴展資料（含頭像）Dto，用於查詢
public class MemberProfileDto
{
    public Guid MemberID { get; set; }

    // 生日
    public DateOnly? BirthDate { get; set; }

    // 性別（字串長度 20 以內，可為 null）
    public string? Gender { get; set; }

    // 頭像 ID，可為 null
    public Guid? AvatarID { get; set; }

    // 頭像圖片網址，可為 null
    public string? AvatarUrl { get; set; }

    // 身高 cm，可為 null
    public int? HeightCm { get; set; }

    // 體重 kg，可為 null
    public int? WeightKg { get; set; }
}
