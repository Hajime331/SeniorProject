using Meow.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// 加入 Cookie Authentication
builder.Services
    .AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Auth/Login";          // 未登入被要求登入時導向的頁面
        options.LogoutPath = "/Auth/Logout";        // 可選：登出路徑
        options.AccessDeniedPath = "/Auth/AccessDenied";  // 可選：權限不足導向
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // Cookie 存活時間
        options.SlidingExpiration = true;               // 期間內活動會延長有效期
        // options.Cookie.Name = "Meow.Auth";          // 可自訂 Cookie 名稱
        // options.Cookie.SameSite = SameSiteMode.Lax; // 預設 Lax 夠用
    });

// 加入授權策略
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireClaim("IsAdmin", "True"));
    // 若你想用角色，也可加：
    // options.AddPolicy("AdminOnlyByRole", p => p.RequireRole("Admin"));
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ForwardAuthCookieHandler>();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient<Meow.Web.Services.IBackendApi, Meow.Web.Services.BackendApi>(client =>
{
    var baseUrl = builder.Configuration["BackendApi:BaseUrl"];
    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException("Missing config: BackendApi:BaseUrl");
    if (!baseUrl.EndsWith("/")) baseUrl += "/"; // 確保結尾有斜線
    client.BaseAddress = new Uri(baseUrl);
})
.AddHttpMessageHandler<ForwardAuthCookieHandler>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // 開啟 Cookie 登入功能
app.UseAuthorization();

// Area 路由
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// 預設路由
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
