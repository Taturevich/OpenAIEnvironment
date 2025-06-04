using Bunit;
using TicketAvailability.UI.Components;
using TicketAvailability.UI.Services;
using TicketAvailability.Web.Grains;
using Microsoft.Extensions.DependencyInjection;
using Orleans.TestingHost;

namespace TicketAvailability.UI.Tests;

public class SeatMapTests : TestContext, IAsyncLifetime
{
    private readonly TestCluster _cluster;
    public SeatMapTests()
    {
        var builder = new TestClusterBuilder(1);
        builder.AddSiloBuilderConfigurator<SiloConfigurator>();
        _cluster = builder.Build();
        _cluster.Deploy();
        Services.AddSingleton<IGrainFactory>(_cluster.GrainFactory);
        Services.AddSingleton<SeatService>();
    }

    [Fact]
    public async Task Clicking_Seat_Locks_It()
    {
        var cut = RenderComponent<SeatMap>();
        var button = cut.Find("button");
        await cut.InvokeAsync(() => button.Click());
        var grain = _cluster.GrainFactory.GetGrain<ISeatGrain>("ui-event-seat1");
        cut.WaitForAssertion(() =>
            Assert.False(grain.IsAvailable().GetAwaiter().GetResult()));
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        _cluster.StopAllSilos();
        return Task.CompletedTask;
    }
}

public class SiloConfigurator : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder)
    {
        siloBuilder.AddMemoryGrainStorage("Default");
    }
}

