using slowpoke.core.Models.Broadcast;
using slowpoke.core.Models.Configuration;

namespace slowpoke.core.Services.Broadcast;

public class StubHttpBroadcastProvider: IHttpBroadcastProvider
{
    public StubHttpBroadcastProvider(Config? config = null)
    {
        Config = config ?? new Config();
    }

    public List<IBroadcastMessage> UnsentMessages => new List<IBroadcastMessage>();

    public List<IBroadcastMessage> SentMessages => new List<IBroadcastMessage>();

    public List<IBroadcastMessage> ReceivedMessages => new List<IBroadcastMessage>();

    public Config Config { get; }

    public bool IsLocalHost => false;

    public Guid OriginGuid => Guid.Empty;

    public Task Publish(IBroadcastMessage message, CancellationToken cancellationToken) => Task.CompletedTask;

    private Task Publish(string knownHost, IBroadcastMessage message, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task<IEnumerable<IBroadcastMessage>> Receive(Guid lastEventReceived, CancellationToken cancellationToken) => Task.FromResult(Enumerable.Empty<IBroadcastMessage>());

    private Task<IEnumerable<IBroadcastMessage>> Receive(string knownHost, Guid lastEventReceived, CancellationToken cancellationToken) => Task.FromResult(Enumerable.Empty<IBroadcastMessage>());
}