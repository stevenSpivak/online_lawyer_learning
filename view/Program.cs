using view.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<view.Services.LawApiClient>(
    client =>
    {
        string apiBaseUrl =
            builder.Configuration["ApiBaseUrl"]
            ?? "http://localhost:5214/";

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