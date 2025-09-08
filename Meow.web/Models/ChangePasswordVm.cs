using System.ComponentModel.DataAnnotations;

public class ChangePasswordVm
{
    [Required, DataType(DataType.Password), Display(Name = "目前密碼")]
    public string CurrentPassword { get; set; } = default!;

    [Required, DataType(DataType.Password), MinLength(6), Display(Name = "新密碼")]
    public string NewPassword { get; set; } = default!;

    [Required, DataType(DataType.Password), Compare(nameof(NewPassword)), Display(Name = "確認新密碼")]
    public string ConfirmNewPassword { get; set; } = default!;
}
