using TicketAvailability.UI.Components;
using Orleans.Hosting;
using TicketAvailability.Web.Grains;
using Microsoft.Extensions.Caching.Memory;
using TicketAvailability.UI.Services;

var builder = WebApplication.CreateBuilder(args);

// Ensure Kestrel uses the expected development port when no URL is specified
const int uiPort = 5162;
builder.WebHost.UseUrls($"http://localhost:{uiPort}");

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<SeatService>();

builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder.UseLocalhostClustering();
    siloBuilder.AddMemoryGrainStorage("Default");
});

var app = builder.Build();

// Redirect to the configured development URL if launched on a different port
app.Use(async (context, next) =>
{
    if (context.Request.Host.Port is int port && port != uiPort)
    {
        var target = $"http://{context.Request.Host.Host}:{uiPort}{context.Request.PathBase}{context.Request.Path}{context.Request.QueryString}";
        context.Response.Redirect(target, permanent: true);
        return;
    }
    await next();
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
