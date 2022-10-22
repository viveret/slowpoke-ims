using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Models.SyncState;

namespace slowpoke.core.Services;


public interface ISyncStateManager
{
    SyncStateModel GetForSystem();
    
    SyncStateModel GetForAction(Guid actionId);
    
    void SetForSystem(SyncStateModel state);

    void SetForAction(Guid actionId, SyncStateModel state);
}