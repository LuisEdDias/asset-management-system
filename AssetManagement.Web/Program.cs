using AssetManagement.Web.Components;
using AssetManagement.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? "http://localhost:7001/";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl.EndsWith("/") ? apiBaseUrl : apiBaseUrl + "/");
});

builder.Services.AddScoped<AssetsApi>();
builder.Services.AddScoped<UsersApi>();
builder.Services.AddScoped<AssetTypesApi>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseStatusCodePagesWithReExecute("/error/{0}");
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapGet("/", (HttpContext context) => {
    context.Response.Redirect("/assets");
    return Task.CompletedTask;
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
