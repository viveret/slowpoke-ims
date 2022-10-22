using System.Collections.Concurrent;
using slowpoke.core.Models.SyncState;

namespace slowpoke.core.Services;


public class SyncStateManager : ISyncStateManager
{
    private static readonly ConcurrentDictionary<Guid, SyncStateModel> SyncStates = new();

    public SyncStateManager()
    {
    }

    public SyncStateModel GetForAction(Guid actionId)
    {
        if (SyncStates.TryGetValue(actionId, out var state))
        {
            return state;
        }
        else
        {
            state = new SyncStateModel();
            SyncStates[actionId] = state;
            return state;
        }
    }

    public SyncStateModel GetForSystem() => GetForAction(Guid.Empty);

    public void SetForAction(Guid actionId, SyncStateModel state) => SyncStates[actionId] = state;

    public void SetForSystem(SyncStateModel state) => SetForAction(Guid.Empty, state);
}