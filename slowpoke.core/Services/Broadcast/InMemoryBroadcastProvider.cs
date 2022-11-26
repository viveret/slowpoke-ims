using System.Collections.Concurrent;
using System.Text;
using slowpoke.core.Models.Broadcast;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Util;
using SlowPokeIMS.Core.Services.Broadcast;

namespace slowpoke.core.Services.Broadcast;



public class InMemoryBroadcastProvider: IInMemoryBroadcastProvider
{
    public InMemoryBroadcastProvider(Config config, IEnumerable<IBroadcastLogger> loggers)
    {
        Config = config;
        Loggers = loggers;
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
    
    public IEnumerable<IBroadcastLogger> Loggers { get; }

    public Task Publish(IBroadcastMessage message, CancellationToken cancellationToken)
    {
        CachedUnsentPublishedMessages.Enqueue(message);
        PersistPublishedMessages[message.EventGuid] = message;

        return WritePersistMessages(PersistPublishedMessages.Values.ToList(), cancellationToken);
    }

    public Task SendUnsentMessages(Action<IBroadcastMessage> send)
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
        return Task.CompletedTask;
    }

    public Task WritePersistMessages(List<IBroadcastMessage> msgs, CancellationToken cancellationToken)
    {
        var msgsProcessed = new List<IBroadcastMessage>();

        foreach (var msg in msgs)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            foreach (var log in Loggers)
            {
                log.Log(msg);
            }

            msgsProcessed.Add(msg);
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<IBroadcastMessage>> ReadPersistedSentMessages(CancellationToken cancellationToken = default)
    {
        return ReadMessagesFromTextFile("broadcast-sent-messages.csv");
    }

    public Task<IEnumerable<IBroadcastMessage>> ReadPersistedReceivedMessages(CancellationToken cancellationToken = default)
    {
        return ReadMessagesFromTextFile("broadcast-received-messages.csv");
    }
    
    public Task<IEnumerable<IBroadcastMessage>> ReadMessagesFromTextFile(string filePath, CancellationToken cancellationToken = default)
    {
        var f = new FileInfo(Path.Combine(Config.Paths.AppRootPath, filePath));
        if (!f.Exists)
        {
            return Task.FromResult(Enumerable.Empty<IBroadcastMessage>());
        }

        using var sr = new StreamReader(f.OpenRead());
        var ret = new List<IBroadcastMessage>();
        while(!sr.EndOfStream)
        {
            var line = sr.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
                continue;
            
            ret.Add(BroadcastMessageRaw.Parse(line).ConvertToTrueType());
        }

        return Task.FromResult<IEnumerable<IBroadcastMessage>>(ret);
    }
    
    public async Task<IEnumerable<IBroadcastMessage>> Receive(Guid lastEventReceived, CancellationToken cancellationToken)
    {
        var idsToReturn = await GetCachedReceivedSince(lastEventReceived);
        return idsToReturn.OrderBy(m => m.BroadcastReceiveDate).ToList();
    }

    private Task<IEnumerable<IBroadcastMessage>> GetCachedReceivedSince(Guid lastEventReceived)
    {
        var idsToDate = CachedReceivedMessages;
        var list = (IEnumerable<IBroadcastMessage>) idsToDate;
        list = list.OrderBy(kvp => kvp.BroadcastReceiveDate ?? DateTime.MaxValue);
        
        if (lastEventReceived != Guid.Empty)
            list = list.SkipWhile(v => v.EventGuid != lastEventReceived);
        
        return Task.FromResult<IEnumerable<IBroadcastMessage>>(list.ToList());
    }

    public Task ExportToStream(Stream destination, bool isTextStream)
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

        return Task.CompletedTask;
    }

    public Task ImportFromStream(Stream source, bool isTextStream)
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

        return Task.CompletedTask;
    }

    public Task AddReceivedMessages(IEnumerable<IBroadcastMessage> messages)
    {
        foreach (var msg in messages)
        {
            CachedReceivedMessages.Enqueue(msg);
        }
        return Task.CompletedTask;
    }
}