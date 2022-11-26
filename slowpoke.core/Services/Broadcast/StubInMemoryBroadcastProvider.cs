using slowpoke.core.Models.Broadcast;

namespace slowpoke.core.Services.Broadcast;

public class StubInMemoryBroadcastProvider : IInMemoryBroadcastProvider
{
    public List<IBroadcastMessage> UnsentMessages => new List<IBroadcastMessage>();

    public List<IBroadcastMessage> SentMessages => new List<IBroadcastMessage>();

    public List<IBroadcastMessage> ReceivedMessages => new List<IBroadcastMessage>();

    public Task AddReceivedMessages(IEnumerable<IBroadcastMessage> messages) => Task.CompletedTask;

    public Task ExportToStream(Stream destination, bool isTextStream) => Task.CompletedTask;

    public Task ImportFromStream(Stream source, bool isTextStream) => Task.CompletedTask;

    public Task Publish(IBroadcastMessage message, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task<IEnumerable<IBroadcastMessage>> ReadPersistedReceivedMessages(CancellationToken cancellationToken = default) => Task.FromResult(Enumerable.Empty<IBroadcastMessage>());

    public Task<IEnumerable<IBroadcastMessage>> ReadPersistedSentMessages(CancellationToken cancellationToken = default) => Task.FromResult(Enumerable.Empty<IBroadcastMessage>());

    public Task<IEnumerable<IBroadcastMessage>> Receive(Guid lastEventReceived, CancellationToken cancellationToken) => Task.FromResult(Enumerable.Empty<IBroadcastMessage>());

    public Task SendUnsentMessages(Action<IBroadcastMessage> send) => Task.CompletedTask;
}