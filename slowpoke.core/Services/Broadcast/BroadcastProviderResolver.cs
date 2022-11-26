using slowpoke.core.Models.Configuration;
using slowpoke.core.Services.Node;
using SlowPokeIMS.Core.Services.Broadcast;

namespace slowpoke.core.Services.Broadcast;



public class BroadcastProviderResolver : IBroadcastProviderResolver
{
    public BroadcastProviderResolver(IEnumerable<IBroadcastProvider> providers)
    {
        All = providers;
        MemCached = providers.OfType<IInMemoryBroadcastProvider>().Single();
        HttpKnownHosts = providers.OfType<IHttpBroadcastProvider>().Single();
    }

    public IEnumerable<IBroadcastProvider> All { get; }

    public IInMemoryBroadcastProvider MemCached { get; }

    public IHttpBroadcastProvider HttpKnownHosts { get; }



    public static BroadcastProviderResolver CreateForTesting(Config cfg, ISlowPokeHostProvider hostProvider)
    {
        var broadcaster = new HttpBroadcastProvider(cfg, hostProvider);
        return new BroadcastProviderResolver(new IBroadcastProvider [] { broadcaster, new InMemoryBroadcastProvider(cfg, new List<IBroadcastLogger>()) });
    }
}