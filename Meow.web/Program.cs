using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Meow.Web.Services;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// === DataProtection�G�P API �@�ΦP�@���_�� ===
var keyRingPath = Path.Combine(builder.Environment.ContentRootPath, "..", "shared-keys");
Directory.CreateDirectory(keyRingPath);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keyRingPath))
    .SetApplicationName("MeowAuth"); // �P API �@�P

// === Cookie Auth�]�P API �����@�P�� Scheme �P Cookie �W�١^ ===
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = ".AspNetCore.Cookies"; // �P API �@�P
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// ���v�����]�O�d�A�쥻�� AdminOnly�^
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireClaim("IsAdmin", "True"));
    // �� roles:
    // options.AddPolicy("AdminOnlyByRole", p => p.RequireRole("Admin"));
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ForwardAuthCookieHandler>();

builder.Services.AddControllersWithViews();

// === Typed HttpClient�G�� Web �� Cookie ��o�� API ===
builder.Services.AddHttpClient<IBackendApi, BackendApi>(client =>
{
    var baseUrl = builder.Configuration["BackendApi:BaseUrl"];
    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException("Missing config: BackendApi:BaseUrl");
    if (!baseUrl.EndsWith("/")) baseUrl += "/";
    client.BaseAddress = new Uri(baseUrl);
})
.AddHttpMessageHandler<ForwardAuthCookieHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Areas
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Default
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
