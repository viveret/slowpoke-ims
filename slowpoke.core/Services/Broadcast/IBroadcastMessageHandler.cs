using slowpoke.core.Models.Broadcast;

namespace slowpoke.core.Services.Broadcast;


public interface IBroadcastMessageHandler
{
    Type[] MessageTypesAllowed { get; }

    void Process(IBroadcastMessage message, CancellationToken cancellationToken);
}