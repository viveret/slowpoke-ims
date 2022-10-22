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
}