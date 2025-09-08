using System.ComponentModel.DataAnnotations;

// 會員個人資料 ViewModel
public class AccountProfileVm
{
    [Display(Name = "Email")]
    public string Email { get; set; } = default!; // 只讀顯示

    [Required, MaxLength(80)]
    [Display(Name = "暱稱")]
    public string Nickname { get; set; } = default!;
}
