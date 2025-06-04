using System.Threading.Tasks;
using Orleans.TestingHost;
using TicketAvailability.Web.Grains;
using Xunit;

namespace TicketAvailability.Tests;

public class SeatGrainTests : IClassFixture<ClusterFixture>
{
    private readonly TestCluster _cluster;
    public SeatGrainTests(ClusterFixture fixture)
    {
        _cluster = fixture.Cluster;
    }

    [Fact]
    public async Task Seat_Is_Available_Initially()
    {
        var grain = _cluster.GrainFactory.GetGrain<ISeatGrain>("event1-seat1");
        var available = await grain.IsAvailable();
        Assert.True(available);
    }

    [Fact]
    public async Task Locking_Same_Seat_Twice_Fails()
    {
        var grain = _cluster.GrainFactory.GetGrain<ISeatGrain>("event1-seat2");
        var first = await grain.TryLockSeat();
        var second = await grain.TryLockSeat();
        Assert.True(first);
        Assert.False(second);
    }
}

public class ClusterFixture : IDisposable
{
    public TestCluster Cluster { get; }

    public ClusterFixture()
    {
        var builder = new TestClusterBuilder(1);
        builder.AddSiloBuilderConfigurator<SiloConfigurator>();
        Cluster = builder.Build();
        Cluster.Deploy();
    }

    public void Dispose()
    {
        Cluster?.StopAllSilos();
    }
}

public class SiloConfigurator : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder)
    {
        siloBuilder.AddMemoryGrainStorage("Default");
    }
}
