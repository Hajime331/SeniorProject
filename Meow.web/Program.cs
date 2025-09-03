using Meow.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// 1) 加入 Cookie Authentication
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";          // 未登入被要求登入時導向的頁面
        options.LogoutPath = "/Auth/Logout";        // 可選：登出路徑
        options.AccessDeniedPath = "/Auth/Denied";  // 可選：權限不足導向
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // Cookie 存活時間
        options.SlidingExpiration = true;               // 期間內活動會延長有效期
        // options.Cookie.Name = "Meow.Auth";          // 可自訂 Cookie 名稱
        // options.Cookie.SameSite = SameSiteMode.Lax; // 預設 Lax 夠用
    });

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient<Meow.Web.Services.IBackendApi, Meow.Web.Services.BackendApi>(client =>
{
    var baseUrl = builder.Configuration.GetSection("Api")["BaseUrl"];
    client.BaseAddress = new Uri(baseUrl!);
});

builder.Services.AddHttpClient<IBackendApi, BackendApi>(client =>
{
    // BaseAddress + "api/Members" → 組成完整請求位址
    var baseUrl = builder.Configuration["BackendApi:BaseUrl"];
    client.BaseAddress = new Uri(baseUrl!);
});


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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
