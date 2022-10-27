using slowpoke.core.Models.Http;

namespace slowpoke.core.Services.Http;


public interface IIpAddressHistoryService
{
    IEnumerable<IpAddressConnectHistoryEvent> GetHistory();
    
    IEnumerable<IpAddressConnectHistoryEvent> GetHistory(DateTime minDate, DateTime maxDate);
    
    IpAddressConnectHistoryEvent AddToHistory(IpAddressConnectHistoryEvent connectEvent);
}