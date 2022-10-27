using Microsoft.Extensions.DependencyInjection;
using slowpoke.core.Models.Broadcast.Messages;
using slowpoke.core.Models.Config;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services.Broadcast;
using slowpoke.core.Services.Node;
using slowpoke.core.Services.Node.Docs;

namespace slowpoke.core.Services.Scheduled.Tasks;


public class ScanLocalNetworkForPeersScheduledTask : IScheduledTask
{
    public Config Config { get; }
    public ISyncStateManager SyncStateManager { get; }
    public ISlowPokeHostProvider SlowPokeHostProvider { get; }
    public IServiceProvider Services { get; }

    public string Title => "Scan local network for peers";

    public bool CanRunConcurrently => false;
    
    public bool CanRunManually => true;

    public ScanLocalNetworkForPeersScheduledTask(
        Config config,
        ISyncStateManager syncStateManager,
        ISlowPokeHostProvider slowPokeHostProvider,
        IServiceProvider services)
    {
        Config = config;
        SyncStateManager = syncStateManager;
        SlowPokeHostProvider = slowPokeHostProvider;
        Services = services;
    }

    public IScheduledTaskContext CreateContext(IScheduledTaskManager scheduledTaskManager)
    {
        var ctx = new GenericScheduledTaskContext(scheduledTaskManager) { Services = Services, TaskTypeName = this.GetType().FullName };
        ctx.SyncState = SyncStateManager.GetForAction(ctx.Id);
        return ctx;
    }

    public Task Execute(IScheduledTaskContext context)
    {
        context.OutputLog.Add($"Scanning local network for peers");

        var list = SlowPokeHostProvider.SearchForLocalNetworkHosts(context, context.CancellationToken);

        context.OutputLog.Add($"Finished scanning, found {list.Hosts.Count()}");

        if (list.Hosts.Any())
        {
            SlowPokeHostProvider.AddNewKnownButUntrustedHosts(list.Hosts, context.CancellationToken);
        }
        
        context.OutputLog.Add("Done!");
        return Task.CompletedTask;
    }
}