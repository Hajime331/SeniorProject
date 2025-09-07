using System.ComponentModel.DataAnnotations;

namespace Meow.Shared.Dtos;

public class ChangePasswordDto
{
    [Required, DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = default!;

    [Required, DataType(DataType.Password), MinLength(6)]
    public string NewPassword { get; set; } = default!;

    [Required, DataType(DataType.Password), Compare(nameof(NewPassword))]
    public string ConfirmNewPassword { get; set; } = default!;
}
