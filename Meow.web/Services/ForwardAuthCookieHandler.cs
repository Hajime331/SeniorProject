using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

public class ForwardAuthCookieHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _ctx;

    public ForwardAuthCookieHandler(IHttpContextAccessor ctx) => _ctx = ctx;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var http = _ctx.HttpContext;
        if (http?.Request?.Cookies is not null)
        {
            // 預設 CookieAuth 的 cookie 名稱，多半是 .AspNetCore.Cookies；若你自訂請改掉
            if (http.Request.Cookies.TryGetValue(".AspNetCore.Cookies", out var cookie))
            {
                // 將瀏覽器 cookie 直接塞到 HttpClient 的 Cookie header
                request.Headers.Add("Cookie", $".AspNetCore.Cookies={cookie}");
            }
        }
        return base.SendAsync(request, cancellationToken);
    }
}
