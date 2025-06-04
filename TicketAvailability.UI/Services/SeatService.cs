using System.Collections.Concurrent;
using TicketAvailability.Web.Grains;
using Orleans;

namespace TicketAvailability.UI.Services;

public class SeatService
{
    private readonly IGrainFactory _grains;
    private readonly ConcurrentDictionary<string, bool> _status = new();
    private const string EventId = "ui-event";

    public SeatService(IGrainFactory grains)
    {
        _grains = grains;
    }

    public IEnumerable<string> SeatIds { get; } = Enumerable.Range(1, 9).Select(i => $"seat{i}");

    public async Task<bool> GetStatusAsync(string seatId)
    {
        if (!_status.TryGetValue(seatId, out var available))
        {
            var grain = _grains.GetGrain<ISeatGrain>($"{EventId}-{seatId}");
            available = await grain.IsAvailable();
            _status[seatId] = available;
        }
        return available;
    }

    public async Task RefreshAsync()
    {
        foreach (var id in SeatIds)
        {
            var grain = _grains.GetGrain<ISeatGrain>($"{EventId}-{id}");
            _status[id] = await grain.IsAvailable();
        }
    }

    public async Task ToggleAsync(string seatId)
    {
        var grain = _grains.GetGrain<ISeatGrain>($"{EventId}-{seatId}");
        if (await GetStatusAsync(seatId))
        {
            var success = await grain.TryLockSeat();
            if (success)
                _status[seatId] = false;
        }
        else
        {
            await grain.UnlockSeat();
            _status[seatId] = true;
        }
    }
}
