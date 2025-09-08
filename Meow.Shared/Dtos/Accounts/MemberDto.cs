namespace Meow.Shared.Dtos.Accounts;

public class MemberDto
{
    public Guid MemberID { get; set; }
    public string Email { get; set; } = default!;
    public string Nickname { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = default!;
    public bool IsAdmin { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

