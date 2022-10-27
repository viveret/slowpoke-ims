using Microsoft.Extensions.Logging;
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
    public DateTime WhenCompleted { get; set; } = DateTime.MinValue;
    public Exception Error { get; set; }
    public List<string> OutputLog { get; set; } = new List<string>();

    public CancellationToken CancellationToken { get; } = CancellationToken.None;

    public bool HasCompleted => WhenCompleted > DateTime.MinValue;

    public IDisposable BeginScope<TState>(TState state)
    {
        throw new NotImplementedException();
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        OutputLog.Add($"{eventId}\t{logLevel}\t{formatter(state, exception)}");
    }
}