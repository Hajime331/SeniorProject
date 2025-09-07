using System.ComponentModel.DataAnnotations;

namespace Meow.Shared.Dtos;

public class MemberLoginDto
{
    [Required, EmailAddress, StringLength(255)]
    public string Email { get; set; } = default!;

    [Required, StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = default!;
}
