namespace Meow.Shared.Dtos.Accounts;

// 系統頭像資料傳輸物件
public class AvatarDto
{
    public Guid AvatarID { get; set; }

    public string Name { get; set; } = default!;

    public string ImageUrl { get; set; } = default!;

    public string? Category { get; set; }

    public string? Tags { get; set; }
}
