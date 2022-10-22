using System.Collections.Concurrent;
using System.Text;
using slowpoke.core.Client;
using slowpoke.core.Models.Broadcast;
using slowpoke.core.Models.Config;
using slowpoke.core.Util;

namespace slowpoke.core.Services.Broadcast;



public class HttpBroadcastProvider: IHttpBroadcastProvider
{
    private static readonly Guid ProcessGuid = Guid.NewGuid();

    public HttpBroadcastProvider(Config config)
    {
        Config = config;
    }

    public List<IBroadcastMessage> UnsentMessages => new List<IBroadcastMessage>();

    public List<IBroadcastMessage> SentMessages => new List<IBroadcastMessage>();

    public List<IBroadcastMessage> ReceivedMessages => new List<IBroadcastMessage>();

    public Config Config { get; }

    public bool IsLocalHost => false;

    public Guid OriginGuid => ProcessGuid;

    public void Publish(IBroadcastMessage message, CancellationToken cancellationToken)
    {
        message.OriginGuid = ProcessGuid;
        var exceptions = new List<Exception>();
        foreach (var knownHost in Config.P2P.KnownHosts)
        {
            try
            {
                Publish(knownHost, message, cancellationToken);
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

    private void Publish(string knownHost, IBroadcastMessage message, CancellationToken cancellationToken)
    {
        using var client = HttpSlowPokeClient.Connect(knownHost, Config, cancellationToken);
        client.Publish(message, cancellationToken);
    }

    public IEnumerable<IBroadcastMessage> Receive(Guid lastEventReceived, CancellationToken cancellationToken)
    {
        var exceptions = new List<Exception>();
        var msgsFromAll = new List<IBroadcastMessage>();
        foreach (var knownHost in Config.P2P.KnownHosts)
        {
            IEnumerable<IBroadcastMessage> msgs;
            try
            {
                msgs = Receive(knownHost, lastEventReceived, cancellationToken);
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

    private IEnumerable<IBroadcastMessage> Receive(string knownHost, Guid lastEventReceived, CancellationToken cancellationToken)
    {
        using var client = HttpSlowPokeClient.Connect(knownHost, Config, cancellationToken);
        return client.Receive(lastEventReceived, cancellationToken);
    }
}