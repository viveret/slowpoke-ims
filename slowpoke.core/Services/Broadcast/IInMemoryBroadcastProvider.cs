using slowpoke.core.Models.Broadcast;

namespace slowpoke.core.Services.Broadcast;



public interface IInMemoryBroadcastProvider: IBroadcastProvider
{
    void SendUnsentMessages(Action<IBroadcastMessage> send);
    //void ProcessReceivedMessages(Action<IBroadcastMessage> send);
    
    void AddReceivedMessages(IEnumerable<IBroadcastMessage> messages);

    void ExportToStream(Stream destination, bool isTextStream);
    
    void ImportFromStream(Stream source, bool isTextStream);

    IEnumerable<IBroadcastMessage> ReadPersistedSentMessages(CancellationToken cancellationToken = default);
    
    IEnumerable<IBroadcastMessage> ReadPersistedReceivedMessages(CancellationToken cancellationToken = default);

    List<IBroadcastMessage> UnsentMessages { get; }
    List<IBroadcastMessage> SentMessages { get; }
    List<IBroadcastMessage> ReceivedMessages { get; }
}