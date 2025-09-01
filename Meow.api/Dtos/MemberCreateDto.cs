// Meow.Api/Dtos/MemberCreateDto.cs
using System.ComponentModel.DataAnnotations;

namespace Meow.Api.Dtos
{
    public class MemberCreateDto
    {
        [Required, EmailAddress, StringLength(255)]
        public string Email { get; set; } = default!;

        [Required, StringLength(80)]
        public string Nickname { get; set; } = default!;

        [Required, StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = default!;
    }
}
