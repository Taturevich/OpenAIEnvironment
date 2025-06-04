using System.Threading.Tasks;
using Orleans;

namespace TicketAvailability.Web.Grains
{
    public class SeatGrain : Grain, ISeatGrain
    {
        private bool _locked;

        public Task<bool> IsAvailable() => Task.FromResult(!_locked);

        public Task UnlockSeat()
        {
            _locked = false;
            return Task.CompletedTask;
        }

        public Task<bool> TryLockSeat()
        {
            if (_locked)
            {
                return Task.FromResult(false);
            }
            _locked = true;
            return Task.FromResult(true);
        }
    }
}
