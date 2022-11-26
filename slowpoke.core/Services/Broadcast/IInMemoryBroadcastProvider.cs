using slowpoke.core.Models.Broadcast;

namespace slowpoke.core.Services.Broadcast;



public interface IInMemoryBroadcastProvider: IBroadcastProvider
{
    Task SendUnsentMessages(Action<IBroadcastMessage> send);
    //void ProcessReceivedMessages(Action<IBroadcastMessage> send);
    
    Task AddReceivedMessages(IEnumerable<IBroadcastMessage> messages);

    Task ExportToStream(Stream destination, bool isTextStream);
    
    Task ImportFromStream(Stream source, bool isTextStream);

    Task<IEnumerable<IBroadcastMessage>> ReadPersistedSentMessages(CancellationToken cancellationToken = default);
    
    Task<IEnumerable<IBroadcastMessage>> ReadPersistedReceivedMessages(CancellationToken cancellationToken = default);

    List<IBroadcastMessage> UnsentMessages { get; }
    List<IBroadcastMessage> SentMessages { get; }
    List<IBroadcastMessage> ReceivedMessages { get; }
}