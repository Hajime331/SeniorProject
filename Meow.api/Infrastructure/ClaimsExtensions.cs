using System;
using System.Security.Claims;

namespace Meow.Api.Infrastructure
{
    public static class ClaimsExtensions
    {
        /// <summary>
        /// 擷取登入會員的 Guid。優先使用自訂 "memberId" claim，
        /// 退而求其次使用 NameIdentifier (sub)。
        /// </summary>
        public static Guid GetMemberId(this ClaimsPrincipal user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            // 自訂 claim
            var v = user.FindFirst("memberId")?.Value
                    ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (Guid.TryParse(v, out var id)) return id;

            throw new InvalidOperationException("Missing or invalid memberId claim.");
        }
    }
}
