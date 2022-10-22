using System.Collections.Concurrent;
using System.Text;
using slowpoke.core.Models.Broadcast;
using slowpoke.core.Models.Config;
using slowpoke.core.Util;

namespace slowpoke.core.Services.Broadcast;



public class InMemoryBroadcastProvider: IInMemoryBroadcastProvider
{
    public InMemoryBroadcastProvider(Config config)
    {
        Config = config;
    }

    private static ConcurrentDictionary<Guid, IBroadcastMessage> CachedSentPublishedMessages { get; } = new ConcurrentDictionary<Guid, IBroadcastMessage>();

    private static ConcurrentQueue<IBroadcastMessage> CachedUnsentPublishedMessages { get; } = new();   
    private static ConcurrentDictionary<Guid, IBroadcastMessage> PersistPublishedMessages { get; } = new();

    private static ConcurrentQueue<IBroadcastMessage> CachedReceivedMessages { get; } = new();
    private static DoubleBufferContainer<ConcurrentDictionary<Guid, IBroadcastMessage>> PersistReceivedMessages { get; } = new();


    public List<IBroadcastMessage> UnsentMessages => CachedUnsentPublishedMessages.ToList();

    public List<IBroadcastMessage> SentMessages => CachedSentPublishedMessages.Values.ToList();

    public List<IBroadcastMessage> ReceivedMessages => CachedReceivedMessages.ToList();

    public Config Config { get; }

    public void Publish(IBroadcastMessage message, CancellationToken cancellationToken)
    {
        CachedUnsentPublishedMessages.Enqueue(message);
        PersistPublishedMessages[message.EventGuid] = message;

        WritePersistMessages(PersistPublishedMessages.Values.ToList(), cancellationToken);
    }

    public void SendUnsentMessages(Action<IBroadcastMessage> send)
    {
        var ignoreFeedback = new List<Guid>();
        while (!CachedUnsentPublishedMessages.IsEmpty && CachedUnsentPublishedMessages.TryDequeue(out var msg))
        {
            if (ignoreFeedback.Contains(msg.EventGuid))
            {
                continue;
            }
            else
            {
                ignoreFeedback.Add(msg.EventGuid);
            }
            send(msg);
            CachedSentPublishedMessages[msg.EventGuid] = msg;
        }
    }

    public void WritePersistMessages(List<IBroadcastMessage> msgs, CancellationToken cancellationToken)
    {
        var msgsProcessed = new List<IBroadcastMessage>();
        var f = new FileInfo(Path.Combine(Config.Paths.AppRootPath, "broadcast-sent-messages.csv"));
        using var destination = f.Open(FileMode.Append);
        foreach (var msg in msgs)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var msgStr = msg.ConvertToRaw().ToString();
            destination.Write(Encoding.ASCII.GetBytes(msgStr!));
            destination.WriteByte((byte)'\n');
            destination.Flush();
            msgsProcessed.Add(msg);
        }
    }

    public IEnumerable<IBroadcastMessage> ReadPersistedSentMessages(CancellationToken cancellationToken = default)
    {
        return ReadMessagesFromTextFile("broadcast-sent-messages.csv");
    }

    public IEnumerable<IBroadcastMessage> ReadPersistedReceivedMessages(CancellationToken cancellationToken = default)
    {
        return ReadMessagesFromTextFile("broadcast-received-messages.csv");
    }
    
    public IEnumerable<IBroadcastMessage> ReadMessagesFromTextFile(string filePath, CancellationToken cancellationToken = default)
    {
        var f = new FileInfo(Path.Combine(Config.Paths.AppRootPath, filePath));
        if (!f.Exists)
        {
            yield break;
        }

        using var sr = new StreamReader(f.OpenRead());
        while(!sr.EndOfStream)
        {
            var line = sr.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
                continue;
            yield return BroadcastMessageRaw.Parse(line).ConvertToTrueType();
        }
    }
    
    public IEnumerable<IBroadcastMessage> Receive(Guid lastEventReceived, CancellationToken cancellationToken)
    {
        var idsToReturn = GetCachedReceivedSince(lastEventReceived);
        return idsToReturn.OrderBy(m => m.BroadcastReceiveDate).ToList();
    }

    private IEnumerable<IBroadcastMessage> GetCachedReceivedSince(Guid lastEventReceived)
    {
        var idsToDate = CachedReceivedMessages;
        var list = (IEnumerable<IBroadcastMessage>) idsToDate;
        list = list.OrderBy(kvp => kvp.BroadcastReceiveDate ?? DateTime.MaxValue);
        
        if (lastEventReceived != Guid.Empty)
            list = list.SkipWhile(v => v.EventGuid != lastEventReceived);
        
        return list.ToList();
    }

    public void ExportToStream(Stream destination, bool isTextStream)
    {
        if (isTextStream)
        {

        }
        foreach (var src in new [] {
            (nameof(this.ReceivedMessages), this.ReceivedMessages),
            (nameof(this.SentMessages), this.SentMessages),
            (nameof(this.UnsentMessages), this.UnsentMessages)
            })
        {
            destination.Write(Encoding.ASCII.GetBytes(src.Item1));
            destination.WriteByte((byte)'\n');
            destination.Write(BitConverter.GetBytes(src.Item2.Count));
            destination.WriteByte((byte)'\n');
            foreach (var msg in src.Item2)
            {
                destination.Write(Encoding.ASCII.GetBytes(((BroadcastMessageRaw) msg.ConvertToRaw()).ToString()));
                destination.WriteByte((byte)'\n');
            }
        }
    }

    public void ImportFromStream(Stream source, bool isTextStream)
    {
        foreach (var src in new [] {
            (nameof(this.ReceivedMessages), this.ReceivedMessages),
            (nameof(this.SentMessages), this.SentMessages),
            (nameof(this.UnsentMessages), this.UnsentMessages)
            })
        {
            /*var name = Encoding.ASCII.GetString()
            destination.Write(Encoding.ASCII.GetBytes(src.Item1));
            destination.WriteByte((byte)'\n');
            destination.Write(BitConverter.GetBytes(src.Item2.Count));
            destination.WriteByte((byte)'\n');
            foreach (var msg in src.Item2)
            {
                ((BroadcastMessageRaw) msg.ConvertToRaw()).Serialize(destination);
                destination.WriteByte((byte)'\n');
            }*/
        }
    }

    public void AddReceivedMessages(IEnumerable<IBroadcastMessage> messages)
    {
        foreach (var msg in messages)
        {
            CachedReceivedMessages.Enqueue(msg);
        }
    }
}