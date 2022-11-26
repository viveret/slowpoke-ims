using Microsoft.Extensions.DependencyInjection;
using slowpoke.core.Models.Broadcast.Messages;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services.Broadcast;
using slowpoke.core.Services.Node;
using slowpoke.core.Services.Node.Docs;
using SlowPokeIMS.Core.Services.Scheduled;

namespace slowpoke.core.Services.Scheduled.Tasks;


public class ScanLocalNetworkForPeersScheduledTask : ScheduledTaskBase
{
    public ISlowPokeHostProvider SlowPokeHostProvider { get; }

    public override string Title => "Scan local network for peers";

    public override bool CanRunConcurrently => false;
    
    public override bool CanRunManually => true;

    public ScanLocalNetworkForPeersScheduledTask(
        Config config,
        ISyncStateManager syncStateManager,
        IServiceProvider services,
        ISlowPokeHostProvider slowPokeHostProvider): base(config, syncStateManager, services)
    {
        SlowPokeHostProvider = slowPokeHostProvider;
    }

    public override async Task Execute(IScheduledTaskContext context)
    {
        context.OutputLog.Add($"Scanning local network for peers");

        var list = await SlowPokeHostProvider.SearchForLocalNetworkHosts(context, context.CancellationToken);

        context.OutputLog.Add($"Finished scanning, found {list.Hosts.Count()}");

        if (list.Hosts.Any())
        {
            await SlowPokeHostProvider.AddNewKnownButUntrustedHosts(list.Hosts, context.CancellationToken);
        }
        
        context.OutputLog.Add("Done!");
    }
}