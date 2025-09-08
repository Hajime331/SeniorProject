using System.ComponentModel.DataAnnotations;

namespace Meow.Web.ViewModels
{
    public class MemberCreateVm
    {
        [Required, EmailAddress, StringLength(255)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(80)]
        [Display(Name = "暱稱")]
        public string Nickname { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        [Display(Name = "確認密碼")]
        [Compare(nameof(Password), ErrorMessage = "兩次密碼不一致")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}