using System.Collections.Concurrent;
using slowpoke.core.Models.SyncState;

namespace slowpoke.core.Services.Scheduled;


public class ScheduledTaskManager : IScheduledTaskManager
{
    private static readonly TimeSpan resultCacheLifespan = TimeSpan.FromHours(1);

    private static readonly ConcurrentDictionary<Guid, IScheduledTaskContext> taskContexts = new();
    
    private static readonly ConcurrentQueue<IScheduledTaskContext> taskExecutionQueue = new();
    
    public IEnumerable<IScheduledTask> Tasks { get; }

    public ScheduledTaskManager(IEnumerable<IScheduledTask> tasks)
    {
        Tasks = tasks.OrderBy(t => t.Title, StringComparer.OrdinalIgnoreCase).ToList();
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
            Tasks.Where(tc => tc.GetType().FullName == taskTypeName).Single();

    public IEnumerable<Type> GetTaskTypes() => Tasks.Select(t => t.GetType());

    // this is still not executing but displaying as executed without error
    public IScheduledTaskContext Execute(IScheduledTask task, bool asynchronous = true, bool immediately = false)
    {
        var ctx = task.CreateContext(this);
        taskContexts[ctx.Id] = ctx;
        if (asynchronous)
        {
            if (immediately)
            {
                taskExecutionQueue.Prepend(ctx);
            }
            else
            {
                taskExecutionQueue.Enqueue(ctx);
            }
        }
        else
        {
            ExecuteNow(ctx);
        }
        return ctx;
    }

    public IScheduledTaskContext GetContext(Guid scheduledTaskContextId) => 
            taskContexts.TryGetValue(scheduledTaskContextId, out var tc) ? tc : null;

    public IEnumerable<IScheduledTaskContext> GetScheduledTaskContexts() => taskContexts.Values.OrderBy(t => t.TaskTypeName).ToList();

    public IEnumerable<IScheduledTaskContext> GetScheduledTaskContextsForTask(Type t) => 
                GetScheduledTaskContextsForTask(t.FullName);

    public IEnumerable<IScheduledTaskContext> GetScheduledTaskContextsForTask(IScheduledTask t) =>
                GetScheduledTaskContextsForTask(t.GetType());

    public IEnumerable<IScheduledTaskContext> GetScheduledTaskContextsForTask(string taskTypeName) =>
                taskContexts.Values.Where(tc => tc.TaskTypeName == taskTypeName);

    public void ExecuteNextQueuedTask()
    {
        if (taskExecutionQueue.TryDequeue(out var t))
        {
            ExecuteNow(t);
        }

        RemoveOldResults();
    }

    private static void ExecuteNow(IScheduledTaskContext t)
    {
        t.WhenStarted = DateTime.UtcNow;
        try
        {
            t.Task.Execute(t);
        }
        catch (Exception e)
        {
            t.Error = e;
        }
        t.WhenCompleted = DateTime.UtcNow;
    }

    private void RemoveOldResults()
    {
        var now = DateTime.UtcNow;
        var old = taskContexts.Values.Where(t => t.HasCompleted && (now - t.WhenCompleted) > resultCacheLifespan).Select(t => t.Id).ToList();
        foreach (var id in old)
        {
            taskContexts.TryRemove(id, out var _);
        }
    }
}