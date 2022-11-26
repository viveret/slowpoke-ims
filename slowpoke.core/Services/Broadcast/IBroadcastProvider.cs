using slowpoke.core.Models.Broadcast;

namespace slowpoke.core.Services.Broadcast;



public interface IBroadcastProvider
{
    Task Publish(IBroadcastMessage message, CancellationToken cancellationToken);
    
    Task<IEnumerable<IBroadcastMessage>> Receive(Guid lastEventReceived, CancellationToken cancellationToken);
}