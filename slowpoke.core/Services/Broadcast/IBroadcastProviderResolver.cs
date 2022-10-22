namespace slowpoke.core.Services.Broadcast;



public interface IBroadcastProviderResolver
{
    IEnumerable<IBroadcastProvider> All { get; }
    
    IHttpBroadcastProvider HttpKnownHosts { get; }
    
    IInMemoryBroadcastProvider MemCached { get; }
}