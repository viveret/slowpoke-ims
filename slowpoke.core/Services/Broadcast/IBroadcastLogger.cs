using slowpoke.core.Models.Broadcast;

namespace SlowPokeIMS.Core.Services.Broadcast;


public interface IBroadcastLogger
{
    void BeginLogging();
    void EndLogging();
    void Log(IBroadcastMessage msg);
}