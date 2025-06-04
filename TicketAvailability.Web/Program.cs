using Microsoft.Extensions.Caching.Memory;
using Orleans.Hosting;
using TicketAvailability.Web.Grains;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder.UseLocalhostClustering();
    siloBuilder.AddMemoryGrainStorage("Default");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/seats/{eventId}/{seatId}", async (IGrainFactory grains, IMemoryCache cache, string eventId, string seatId) =>
{
    var key = $"{eventId}-{seatId}";
    if (!cache.TryGetValue(key, out bool available))
    {
        var grain = grains.GetGrain<ISeatGrain>(key);
        available = await grain.IsAvailable();
        cache.Set(key, available);
    }
    return Results.Ok(new { SeatId = seatId, Available = available });
});

app.MapPost("/seats/{eventId}/{seatId}/lock", async (IGrainFactory grains, IMemoryCache cache, string eventId, string seatId) =>
{
    var key = $"{eventId}-{seatId}";
    var grain = grains.GetGrain<ISeatGrain>(key);
    var success = await grain.TryLockSeat();
    cache.Set(key, false);
    return success ? Results.Ok() : Results.Conflict();
});

app.MapPost("/seats/{eventId}/{seatId}/unlock", async (IGrainFactory grains, IMemoryCache cache, string eventId, string seatId) =>
{
    var key = $"{eventId}-{seatId}";
    var grain = grains.GetGrain<ISeatGrain>(key);
    await grain.UnlockSeat();
    cache.Set(key, true);
    return Results.Ok();
});

app.Run();
