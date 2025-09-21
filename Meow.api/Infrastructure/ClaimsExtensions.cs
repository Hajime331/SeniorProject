using System;
using System.Security.Claims;

namespace Meow.Api.Infrastructure
{
    public static class ClaimsExtensions
    {
        public static Guid GetMemberId(this ClaimsPrincipal user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var v = user.FindFirst("memberId")?.Value
                    ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (Guid.TryParse(v, out var id)) return id;

            throw new InvalidOperationException("Missing or invalid memberId claim.");
        }

        /// <summary>
        /// 判斷目前登入者是否為管理員
        /// </summary>
        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            if (user == null) return false;
            // 假設你有在登入時加上 "isAdmin" claim，值是 "true" / "false"
            var v = user.FindFirst("isAdmin")?.Value;
            if (bool.TryParse(v, out var result)) return result;

            // 或者用 Role 判斷
            return user.IsInRole("Admin");
        }
    }
}
