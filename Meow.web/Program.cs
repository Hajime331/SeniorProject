using Meow.Web.Services;

var builder = WebApplication.CreateBuilder(args);

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
