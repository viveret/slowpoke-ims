namespace slowpoke.core.Models.Http; 


// todo: could be byte packed so it can be stored more efficiently
public class IpAddressConnectHistoryEvent
{
    public Guid EventId { get; set; }
    public DateTime WhenConnected { get; set; }
    public string IpAddress { get; set; }
    public Guid IdentityId { get; set; }
    public Guid AuthId { get; set; }
}