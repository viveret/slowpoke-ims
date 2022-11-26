using System.Collections.Concurrent;
using slowpoke.core.Models.SyncState;

namespace slowpoke.core.Services.Scheduled;


public class ScheduledTaskContextStorage
{
    private static readonly TimeSpan resultCacheLifespan = TimeSpan.FromHours(1);

    // TODO: replace static readonly with dependency injected service to singleton with no dependencies,
    // that way you can host multiple sites int he same process without overriding other sinstances memory
    public readonly ConcurrentDictionary<Guid, IScheduledTaskContext> taskContexts = new();
    
    public readonly ConcurrentQueue<IScheduledTaskContext> taskExecutionQueue = new();
    
    public readonly ConcurrentQueue<IScheduledTaskContext> taskExecutionQueueImmediately = new();
    
    public void ClearStaticVars()
    {
        taskContexts.Clear();
        taskExecutionQueue.Clear();
        taskExecutionQueueImmediately.Clear();
    }

    public IScheduledTaskContext GetContext(Guid scheduledTaskContextId) => 
            taskContexts.TryGetValue(scheduledTaskContextId, out var tc) ? tc : null;

    public IEnumerable<IScheduledTaskContext> GetScheduledTaskContexts() => taskContexts.Values.OrderBy(t => t.TaskTypeName).ToList();

    public IEnumerable<IScheduledTaskContext> GetScheduledTaskContextsForTask(Type t) => 
                GetScheduledTaskContextsForTask(t.FullName!);

    public IEnumerable<IScheduledTaskContext> GetScheduledTaskContextsForTask(IScheduledTask t) =>
                GetScheduledTaskContextsForTask(t.GetType());

    public IEnumerable<IScheduledTaskContext> GetScheduledTaskContextsForTask(string taskTypeName) =>
                taskContexts.Values.Where(tc => tc.TaskTypeName == taskTypeName);

    public async Task ExecuteNextQueuedTask()
    {
        if (taskExecutionQueueImmediately.TryDequeue(out var t) ||
            taskExecutionQueue.TryDequeue(out t))
        {
            await ExecuteNow(t);
        }

        RemoveOldResults();
    }

    public async Task ExecuteNow(IScheduledTaskContext t)
    {
        try
        {
            await t.Task.OnStart(t);
        }
        catch (Exception e)
        {
            t.Error = e;
            return;
        }

        try
        {
            await t.Task.Execute(t);
        }
        catch (Exception e)
        {
            t.Error = e;
        }

        try
        {
            await t.Task.OnEnd(t);
        }
        catch (Exception e)
        {
            if (t.Error == null)
            {
                t.Error = e;
            }
            else
            {
                Console.Error.WriteLine(e.Message);
                Console.Error.WriteLine(e.StackTrace);
            }
        }
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

    public Task<bool> HasQueuedTask() => Task.FromResult(taskExecutionQueue.Any() || taskExecutionQueueImmediately.Any());
}