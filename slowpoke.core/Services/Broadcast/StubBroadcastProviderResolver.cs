namespace slowpoke.core.Services.Broadcast;



public class StubBroadcastProviderResolver : IBroadcastProviderResolver
{
    public StubBroadcastProviderResolver()
    {
        All = Enumerable.Empty<IBroadcastProvider>();
        MemCached = new StubInMemoryBroadcastProvider();
        HttpKnownHosts = new StubHttpBroadcastProvider();
    }

    public IEnumerable<IBroadcastProvider> All { get; }

    public IInMemoryBroadcastProvider MemCached { get; }

    public IHttpBroadcastProvider HttpKnownHosts { get; }
}