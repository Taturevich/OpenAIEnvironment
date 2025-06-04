using System.Linq;
using System.Threading.Tasks;
using Orleans.TestingHost;
using TicketAvailability.Web.Grains;
using Xunit;

namespace TicketAvailability.Tests;

public class LargeVenueTests : IClassFixture<ClusterFixture>
{
    private readonly TestCluster _cluster;
    public LargeVenueTests(ClusterFixture fixture)
    {
        _cluster = fixture.Cluster;
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(5000)]
    public async Task Seats_For_Large_Venue_Can_Be_Loaded(int seatCount)
    {
        var tasks = Enumerable.Range(1, seatCount).Select(async i =>
        {
            var grain = _cluster.GrainFactory.GetGrain<ISeatGrain>($"bigEvent-seat{i}");
            return await grain.IsAvailable();
        });

        var results = await Task.WhenAll(tasks);
        Assert.All(results, Assert.True);
    }

    [Fact]
    public async Task All_Seats_Can_Be_Locked_Independently()
    {
        const int seatCount = 1000;
        var tasks = Enumerable.Range(1, seatCount).Select(async i =>
        {
            var grain = _cluster.GrainFactory.GetGrain<ISeatGrain>($"bigEvent2-seat{i}");
            return await grain.TryLockSeat();
        });

        var results = await Task.WhenAll(tasks);
        Assert.All(results, Assert.True);
    }
}
