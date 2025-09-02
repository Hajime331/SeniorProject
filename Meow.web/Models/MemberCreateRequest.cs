namespace Meow.Web.Models
{
    public class MemberCreateRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}