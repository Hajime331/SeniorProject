using System.ComponentModel.DataAnnotations;

public class RegisterVm
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, StringLength(80, MinimumLength = 2)]
    public string Nickname { get; set; } = "";

    [Required, MinLength(6)]
    public string Password { get; set; } = "";

    [Required, Compare(nameof(Password), ErrorMessage = "兩次密碼不一致")]
    public string ConfirmPassword { get; set; } = "";

    // 註冊後要回去的頁面（選用）
    public string? ReturnUrl { get; set; }
}
