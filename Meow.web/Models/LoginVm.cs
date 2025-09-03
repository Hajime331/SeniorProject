using System.ComponentModel.DataAnnotations;

namespace Meow.Web.Models
{
    public class LoginVm
    {
        [Required, EmailAddress, StringLength(255)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "記住我")]
        public bool RememberMe { get; set; } = false;

        public string? ReturnUrl { get; set; }
    }
}
