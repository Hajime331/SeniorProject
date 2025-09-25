using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Meow.Api.Data;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// === DataProtection�G�P Web �@�ΦP�@���_�� ===
// ��ĳ�ε�����|�]��ӪA�ȳ���s���^
var keyRingPath = Path.Combine(builder.Environment.ContentRootPath, "..", "shared-keys");
Directory.CreateDirectory(keyRingPath);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keyRingPath))
    .SetApplicationName("MeowAuth"); // �P Web �@�P

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// === Cookie Auth�G�P�W Cookie�FAPI ���n Redirect�A�����^ 401/403 ===
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o =>
    {
        o.Cookie.Name = ".AspNetCore.Cookies"; // �P Web �@�P

        // API ���� Redirect ��n�J���A�_�h HttpClient �|��� 302 -> 404
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

        // �R���o����קK����۰ʾɭ�
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

app.UseAuthentication(); // �����b UseAuthorization ���e
app.UseAuthorization();

app.UseStaticFiles();

app.MapControllers();

app.Run();
