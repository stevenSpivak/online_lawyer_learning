using view.Components;
using view.Services;

var builder = WebApplication.CreateBuilder(args);

// קוראים את כתובת ה-API מתוך appsettings.json.
string apiBaseUrl =
    builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException(
        "ApiBaseUrl not found in appsettings.json.");


// CHANGED:
// HttpClient רגיל עבור Register.razor,
// Login וכו'.
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
});
builder.Services.AddScoped<UserSession>();

// CHANGED:
// LawApiClient ממשיך לעבוד עבור מאגר החוקים.
builder.Services.AddHttpClient<view.Services.LawApiClient>(
    client =>
    {
        client.BaseAddress = new Uri(apiBaseUrl);
    });

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Error", createScopeForErrors: true);

    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute(
    "/not-found",
    createScopeForStatusCodePages: true);

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();