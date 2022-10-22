using slowpoke.core.Models.Broadcast;

namespace slowpoke.core.Services.Broadcast;



public interface IBroadcastProvider
{
    void Publish(IBroadcastMessage message, CancellationToken cancellationToken);
    
    IEnumerable<IBroadcastMessage> Receive(Guid lastEventReceived, CancellationToken cancellationToken);
}