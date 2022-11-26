using System.Collections.Concurrent;
using System.Text;
using slowpoke.core.Client;
using slowpoke.core.Models.Broadcast;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Services.Node;
using slowpoke.core.Util;

namespace slowpoke.core.Services.Broadcast;



public class HttpBroadcastProvider: IHttpBroadcastProvider
{
    private static readonly Guid ProcessGuid = Guid.NewGuid();

    private readonly ISlowPokeHostProvider slowPokeHostProvider;

    public HttpBroadcastProvider(
        Config config,
        ISlowPokeHostProvider slowPokeHostProvider)
    {
        Config = config;
        this.slowPokeHostProvider = slowPokeHostProvider;
    }

    public List<IBroadcastMessage> UnsentMessages => new List<IBroadcastMessage>();

    public List<IBroadcastMessage> SentMessages => new List<IBroadcastMessage>();

    public List<IBroadcastMessage> ReceivedMessages => new List<IBroadcastMessage>();

    public Config Config { get; }

    public bool IsLocalHost => false;

    public Guid OriginGuid => ProcessGuid;

    public async Task Publish(IBroadcastMessage message, CancellationToken cancellationToken)
    {
        message.OriginGuid = ProcessGuid;
        var exceptions = new List<Exception>();
        foreach (var knownHost in slowPokeHostProvider.AllExceptCurrent)
        {
            try
            {
                await Publish(knownHost.Endpoint, message, cancellationToken);
            }
            catch (Exception e)
            {
                // todo: probably want to requeue message so that system resends for this specific host
                exceptions.Add(new Exception($"Could not publish to {knownHost}", e));
            }
        }

        if (exceptions.Any())
        {
            throw new AggregateException(exceptions);
        }
    }

    private async Task Publish(Uri knownHost, IBroadcastMessage message, CancellationToken cancellationToken)
    {
        using var client = await slowPokeHostProvider.OpenClient(knownHost, cancellationToken: cancellationToken);
        await client.Publish(message, cancellationToken);
    }

    public async Task<IEnumerable<IBroadcastMessage>> Receive(Guid lastEventReceived, CancellationToken cancellationToken)
    {
        var exceptions = new List<Exception>();
        var msgsFromAll = new List<IBroadcastMessage>();
        foreach (var knownHost in slowPokeHostProvider.AllExceptCurrent)
        {
            IEnumerable<IBroadcastMessage> msgs;
            try
            {
                msgs = await Receive(knownHost.Endpoint, lastEventReceived, cancellationToken);
            }
            catch (Exception e)
            {
                // probably want to requeue message so that system resends for this specific host
                exceptions.Add(new Exception($"Could not publish to {knownHost}", e));
                continue;
            }

            foreach (var msg in msgs)
            {
                if (Guid.Empty == msg.OriginGuid)
                {
                    exceptions.Add(new Exception($"Invalid {nameof(msg.OriginGuid)} for event {msg.EventGuid}"));
                    break;
                }
                else if (ProcessGuid == msg.OriginGuid)
                {
                    msg.OriginGuid = Guid.Empty;
                }
                msgsFromAll.Add(msg);
            }
        }

        if (exceptions.Any())
        {
            throw new AggregateException(exceptions);
        }
        return msgsFromAll;
    }

    private async Task<IEnumerable<IBroadcastMessage>> Receive(Uri knownHost, Guid lastEventReceived, CancellationToken cancellationToken)
    {
        using var client = await slowPokeHostProvider.OpenClient(knownHost, cancellationToken: cancellationToken);
        return await client.Receive(lastEventReceived, cancellationToken);
    }
}