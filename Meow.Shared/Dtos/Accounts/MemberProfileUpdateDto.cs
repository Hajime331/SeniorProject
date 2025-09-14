using System.ComponentModel.DataAnnotations;

namespace Meow.Shared.Dtos.Accounts;

// 會員擴展資料更新用 Dto
public class MemberProfileUpdateDto
{
    [DataType(DataType.Date)]
    public DateOnly? BirthDate { get; set; }

    [StringLength(20)]
    public string? Gender { get; set; }

    [Range(1, 300)]
    public int? HeightCm { get; set; }

    [Range(1, 500)]
    public int? WeightKg { get; set; }
}
