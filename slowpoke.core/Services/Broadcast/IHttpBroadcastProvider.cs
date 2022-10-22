namespace slowpoke.core.Services.Broadcast;



public interface IHttpBroadcastProvider: IBroadcastProvider
{
    bool IsLocalHost { get; }

    Guid OriginGuid { get; }
}