using Meow.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// 1) �[�J Cookie Authentication
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";          // ���n�J�Q�n�D�n�J�ɾɦV������
        options.LogoutPath = "/Auth/Logout";        // �i��G�n�X���|
        options.AccessDeniedPath = "/Auth/Denied";  // �i��G�v�������ɦV
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // Cookie �s���ɶ�
        options.SlidingExpiration = true;               // ���������ʷ|�������Ĵ�
        // options.Cookie.Name = "Meow.Auth";          // �i�ۭq Cookie �W��
        // options.Cookie.SameSite = SameSiteMode.Lax; // �w�] Lax ����
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
    // BaseAddress + "api/Members" �� �զ�����ШD��}
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

app.UseAuthentication(); // �}�� Cookie �n�J�\��
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
