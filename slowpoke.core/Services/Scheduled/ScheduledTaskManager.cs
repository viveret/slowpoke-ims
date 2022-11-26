using System.Collections.Concurrent;
using slowpoke.core.Models.SyncState;

namespace slowpoke.core.Services.Scheduled;


public class ScheduledTaskManager : IScheduledTaskManager
{
    private static readonly TimeSpan resultCacheLifespan = TimeSpan.FromHours(1);

    public ScheduledTaskContextStorage TaskContexts { get; }
    
    public IEnumerable<IScheduledTask> Tasks { get; }

    public ScheduledTaskManager(
        ScheduledTaskContextStorage scheduledTaskContextStorage,
        IEnumerable<IScheduledTask> tasks)
    {
        TaskContexts = scheduledTaskContextStorage;
        Tasks = tasks.OrderBy(t => t.Title, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public static IScheduledTaskManager Empty()
    {
        return new ScheduledTaskManager(new ScheduledTaskContextStorage(), Enumerable.Empty<IScheduledTask>());
    }

    // public IScheduledTask Execute<T>() where T : IScheduledTask
    // {
    //     var t = Tasks.OfType<T>().Single();
    //     return (T) Execute(t);
    // }

    // public IScheduledTask Execute(Type taskType)
    // {
    //     throw new NotImplementedException();
    // }

    public IEnumerable<IScheduledTask> GetScheduledTasks() => Tasks;

    public IScheduledTask GetScheduledTask(string taskTypeName) => 
            Tasks.Where(tc => tc.GetType().FullName == taskTypeName).SingleOrDefault() ?? throw new Exception($"Task not found: {taskTypeName}");

    public IEnumerable<Type> GetTaskTypes() => Tasks.Select(t => t.GetType());

    // this is still not executing but displaying as executed without error
    public async Task<IScheduledTaskContext> Execute(IScheduledTask task, bool asynchronous = true, bool immediately = false)
    {
        var ctx = task.CreateContext(this);
        TaskContexts.taskContexts[ctx.Id] = ctx;
        if (asynchronous)
        {
            if (immediately)
            {
                TaskContexts.taskExecutionQueueImmediately.Enqueue(ctx);
            }
            else
            {
                TaskContexts.taskExecutionQueue.Enqueue(ctx);
            }
        }
        else
        {
            await TaskContexts.ExecuteNow(ctx);
        }
        return ctx;
    }

    public IScheduledTaskContext GetContext(Guid scheduledTaskContextId) => TaskContexts.GetContext(scheduledTaskContextId);

    public IEnumerable<IScheduledTaskContext> GetScheduledTaskContexts() => TaskContexts.GetScheduledTaskContexts();

    public IEnumerable<IScheduledTaskContext> GetScheduledTaskContextsForTask(Type t) => TaskContexts.GetScheduledTaskContextsForTask(t);

    public IEnumerable<IScheduledTaskContext> GetScheduledTaskContextsForTask(IScheduledTask t) => TaskContexts.GetScheduledTaskContextsForTask(t);

    public IEnumerable<IScheduledTaskContext> GetScheduledTaskContextsForTask(string taskTypeName) => TaskContexts.GetScheduledTaskContextsForTask(taskTypeName);

    public Task ExecuteNextQueuedTask() => TaskContexts.ExecuteNextQueuedTask();

    public Task<bool> HasQueuedTask() => TaskContexts.HasQueuedTask();
}