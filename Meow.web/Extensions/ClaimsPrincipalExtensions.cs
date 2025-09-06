using System.Security.Claims;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var s = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(s, out var id))
            throw new InvalidOperationException("使用者識別無效：缺少或格式錯誤的 NameIdentifier。");
        return id;
    }
}