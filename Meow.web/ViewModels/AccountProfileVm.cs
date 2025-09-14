using Meow.Shared.Dtos.Accounts;
using System.ComponentModel.DataAnnotations;

// 會員個人資料 ViewModel（含擴展資訊）
public class AccountProfileVm
{
    [Display(Name = "Email")]
    public string Email { get; set; } = default!; // 只讀顯示

    [Required, MaxLength(80)]
    [Display(Name = "暱稱")]
    public string Nickname { get; set; } = default!;

    [Display(Name = "生日")]
    [DataType(DataType.Date)]
    public DateOnly? BirthDate { get; set; }

    [Display(Name = "性別")]
    [MaxLength(20)]
    public string? Gender { get; set; }

    [Display(Name = "身高 (cm)")]
    [Range(1, 300)]
    public int? HeightCm { get; set; }

    [Display(Name = "體重 (kg)")]
    [Range(1, 500)]
    public int? WeightKg { get; set; }

    // 目前已選定的頭像 ID
    public Guid? AvatarID { get; set; }

    // 目前頭像圖片網址（顯示用）
    public string? AvatarUrl { get; set; }

    // 可供選擇的系統頭像清單
    public List<AvatarDto>? Avatars { get; set; }
}
