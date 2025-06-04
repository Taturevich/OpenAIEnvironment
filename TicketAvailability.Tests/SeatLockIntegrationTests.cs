using System.Linq;
using System.Threading.Tasks;
using Orleans.TestingHost;
using TicketAvailability.Web.Grains;
using Xunit;

namespace TicketAvailability.Tests;

public class SeatLockIntegrationTests : IClassFixture<ClusterFixture>
{
    private readonly TestCluster _cluster;
    public SeatLockIntegrationTests(ClusterFixture fixture)
    {
        _cluster = fixture.Cluster;
    }

    [Fact]
    public async Task Only_One_Succeeds_Under_Contention()
    {
        var tasks = Enumerable.Range(0, 10).Select(async _ =>
        {
            var grain = _cluster.GrainFactory.GetGrain<ISeatGrain>("event2-seat1");
            return await grain.TryLockSeat();
        });

        var results = await Task.WhenAll(tasks);
        Assert.Equal(1, results.Count(r => r));
    }
}
