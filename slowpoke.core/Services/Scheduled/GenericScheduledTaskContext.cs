using slowpoke.core.Models.SyncState;

namespace slowpoke.core.Services.Scheduled;


public class GenericScheduledTaskContext: IScheduledTaskContext
{
    private readonly IScheduledTaskManager scheduledTaskManager;

    public GenericScheduledTaskContext(IScheduledTaskManager scheduledTaskManager)
    {
        this.scheduledTaskManager = scheduledTaskManager;
    }

    public IScheduledTask Task => scheduledTaskManager.GetScheduledTask(TaskTypeName);

    public Guid Id { get; set; } = Guid.NewGuid();

    public SyncStateModel SyncState { get; set; }    
    public string TaskTypeName { get; set; }
    public IServiceProvider Services { get; set; }
    public DateTime WhenStarted { get; set; }
    public DateTime WhenCompleted { get; set; }
    public Exception Error { get; set; }
    public List<string> Log { get; set; } = new List<string>();

    public CancellationToken CancellationToken { get; } = CancellationToken.None;
}