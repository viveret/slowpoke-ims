using System.Collections.Concurrent;
using slowpoke.core.Models.Http;

namespace slowpoke.core.Services.Http;


public class IpAddressHistoryService : IIpAddressHistoryService
{
    private static readonly ConcurrentQueue<IpAddressConnectHistoryEvent> history = new ();

    public IpAddressConnectHistoryEvent AddToHistory(IpAddressConnectHistoryEvent connectEvent)
    {
        connectEvent.EventId = new Guid();
        history.Enqueue(connectEvent);
        return connectEvent;
    }

    public IEnumerable<IpAddressConnectHistoryEvent> GetHistory()
    {
        return history.ToList();
    }

    public IEnumerable<IpAddressConnectHistoryEvent> GetHistory(DateTime minDate, DateTime maxDate)
    {
        return history.Where(e => e.WhenConnected >= minDate && e.WhenConnected < maxDate).ToList();
    }
}