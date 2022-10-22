using slowpoke.core.Models.Broadcast;

namespace slowpoke.core.Services.Broadcast;



public interface IBroadcastMessageHandlerResolver
{
    IBroadcastMessageHandler ResolveForType(Type t);
    
    void Handle(IBroadcastMessage message, CancellationToken cancellationToken);
}