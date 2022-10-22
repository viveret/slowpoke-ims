namespace slowpoke.core.Services.Scheduled;


public interface IScheduledTask
{
    string Title { get; }

    bool CanRunConcurrently { get; }

    bool CanRunManually { get; }
    
    IScheduledTaskContext CreateContext(IScheduledTaskManager scheduledTaskManager);
    
    Task Execute(IScheduledTaskContext context);
}