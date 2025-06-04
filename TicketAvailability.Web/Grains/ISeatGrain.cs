using System.Threading.Tasks;
using Orleans;

namespace TicketAvailability.Web.Grains
{
    public interface ISeatGrain : IGrainWithStringKey
    {
        Task<bool> TryLockSeat();
        Task UnlockSeat();
        Task<bool> IsAvailable();
    }
}
