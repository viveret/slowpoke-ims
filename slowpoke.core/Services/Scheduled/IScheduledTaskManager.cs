using slowpoke.core.Models.SyncState;

namespace slowpoke.core.Services.Scheduled;


public interface IScheduledTaskManager
{
    IEnumerable<Type> GetTaskTypes();

    IEnumerable<IScheduledTask> GetScheduledTasks();

    IScheduledTask GetScheduledTask(string taskTypeName);

    IEnumerable<IScheduledTaskContext> GetScheduledTaskContexts();

    IEnumerable<IScheduledTaskContext> GetScheduledTaskContextsForTask(string taskTypeName);

    IEnumerable<IScheduledTaskContext> GetScheduledTaskContextsForTask(Type t);

    IEnumerable<IScheduledTaskContext> GetScheduledTaskContextsForTask(IScheduledTask t);

    IScheduledTaskContext Execute(IScheduledTask task, bool asynchronous = true, bool immediately = false);
    
    void ExecuteNextQueuedTask();

    IScheduledTaskContext GetContext(Guid scheduledTaskContextId);
}