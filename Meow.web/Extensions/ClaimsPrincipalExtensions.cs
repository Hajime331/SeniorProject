using System.Security.Claims;

// 擴充方法類別
public static class ClaimsPrincipalExtensions
{
    // 取得使用者 ID（Guid）
    // 從 ClaimsPrincipal 物件中擷取 NameIdentifier 聲明，並將其轉換為 Guid。如果找不到該聲明，則會引發例外。
    // 如果找不到該聲明，則會引發例外。
    public static Guid GetUserId(this ClaimsPrincipal user)
        => Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
}