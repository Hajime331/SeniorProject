using Meow.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// �[�J Cookie Authentication
builder.Services
    .AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Auth/Login";          // ���n�J�Q�n�D�n�J�ɾɦV������
        options.LogoutPath = "/Auth/Logout";        // �i��G�n�X���|
        options.AccessDeniedPath = "/Auth/AccessDenied";  // �i��G�v�������ɦV
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // Cookie �s���ɶ�
        options.SlidingExpiration = true;               // ���������ʷ|�������Ĵ�
        // options.Cookie.Name = "Meow.Auth";          // �i�ۭq Cookie �W��
        // options.Cookie.SameSite = SameSiteMode.Lax; // �w�] Lax ����
    });

// �[�J���v����
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireClaim("IsAdmin", "True"));
    // �Y�A�Q�Ψ���A�]�i�[�G
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
    if (!baseUrl.EndsWith("/")) baseUrl += "/"; // �T�O�������׽u
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

app.UseAuthentication(); // �}�� Cookie �n�J�\��
app.UseAuthorization();

// Area ����
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// �w�]����
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
