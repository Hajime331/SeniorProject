using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Meow.Api.Data;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// === DataProtection：與 Web 共用同一把鑰匙 ===
// 建議用絕對路徑（兩個服務都能存取）
var keyRingPath = Path.Combine(builder.Environment.ContentRootPath, "..", "shared-keys");
Directory.CreateDirectory(keyRingPath);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keyRingPath))
    .SetApplicationName("MeowAuth"); // 與 Web 一致

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// === Cookie Auth：同名 Cookie；API 不要 Redirect，直接回 401/403 ===
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o =>
    {
        o.Cookie.Name = ".AspNetCore.Cookies"; // 與 Web 一致

        // API 不能 Redirect 到登入頁，否則 HttpClient 會跟著 302 -> 404
        o.Events.OnRedirectToLogin = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        o.Events.OnRedirectToAccessDenied = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };

        // 刪除這兩條避免任何自動導頁
        o.LoginPath = null;
        o.AccessDeniedPath = null;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.IsInRole("Admin") ||
            string.Equals(ctx.User.FindFirst("isAdmin")?.Value, "true", StringComparison.OrdinalIgnoreCase)
        ));
});

// DB
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("AppDb")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication(); // 必須在 UseAuthorization 之前
app.UseAuthorization();

app.UseStaticFiles();

app.MapControllers();

app.Run();
