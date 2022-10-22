using slowpoke.core.Models.SyncState;

namespace slowpoke.core.Services.Scheduled;


public interface IScheduledTaskContext
{
    Guid Id { get; set; }
    
    string TaskTypeName { get; set; } // used to resolve task since context persists in static variable while tasks are transient

    SyncStateModel SyncState { get; set; }
    
    IScheduledTask Task { get; }

    CancellationToken CancellationToken { get; }
    
    IServiceProvider Services { get; set; }

    DateTime WhenStarted { get; set; }
    
    DateTime WhenCompleted { get; set; }

    Exception Error { get; set; }

    List<string> Log { get; set; }
}