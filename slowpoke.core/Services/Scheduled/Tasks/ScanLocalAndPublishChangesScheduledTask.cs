using Microsoft.Extensions.DependencyInjection;
using slowpoke.core.Models.Broadcast.Messages;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services.Broadcast;
using slowpoke.core.Services.Node.Docs;
using SlowPokeIMS.Core.Services.Scheduled;

namespace slowpoke.core.Services.Scheduled.Tasks;


public class ScanLocalAndPublishChangesScheduledTask : ScheduledTaskBase
{
    public IDocumentProviderResolver DocumentProviderResolver { get; }
    public IBroadcastProviderResolver BroadcastProviderResolver { get; }

    public override string Title => "Scan local and publish changes";

    public override bool CanRunConcurrently => false;
    
    public override bool CanRunManually => true;

    public ScanLocalAndPublishChangesScheduledTask(
        Config config,
        ISyncStateManager syncStateManager,
        IServiceProvider services,
        IDocumentProviderResolver documentProviderResolver,
        IBroadcastProviderResolver broadcastProviderResolver): base(config, syncStateManager, services)
    {
        DocumentProviderResolver = documentProviderResolver;
        BroadcastProviderResolver = broadcastProviderResolver;
    }

    public override async Task Execute(IScheduledTaskContext context)
    {
        var remotes = await DocumentProviderResolver.ReadRemotes;
        if (!remotes.Any())
        {
            throw new Exception("No remotes, cannot sync");
        }

        var rl = await DocumentProviderResolver.ReadLocal;
        var query = new QueryDocumentOptions { SyncEnabled = true, Recursive = true, Path = Config.Paths.HomePath.AsIDocPath(Config) };
        var localNodesCount = await rl.GetCountOfNodes(query, context.CancellationToken);
        var localNodes = await rl.GetNodes(query, context.CancellationToken);
        context.OutputLog.Add($"Scanning for changes in {localNodesCount} local nodes that have enabled sync");

        int scanCount = 0, uptodateCount = 0, docChangedCount = 0, docMetaChangedCount = 0;
        var exceptions = new List<Exception>();
        foreach (var node in localNodes)
        {
            try
            {
                var meta = await node.HasMeta ? await node.Meta : null;
                if (meta != null && await meta.MetaExists)
                {
                    if (meta.LastSyncDate.HasValue)
                    {
                        if (meta.LastSyncDate < meta.LastUpdate)
                        {
                            var msg = new DocumentChangedBroadcastMessage(await node.GetFingerprint(context.CancellationToken));
                            await BroadcastProviderResolver.MemCached.Publish(msg, context.CancellationToken);
                            docChangedCount++;
                        }
                        else if (meta.LastSyncDate < meta.LastUpdate)
                        {
                            var msg = new DocMetaChangedBroacastMessage(await node.GetFingerprint(context.CancellationToken));
                            await BroadcastProviderResolver.MemCached.Publish(msg, context.CancellationToken);
                            docMetaChangedCount++;
                        }
                        else
                        {
                            uptodateCount++;
                        }
                    }
                    else
                    {
                        var msg = new SyncStartedBroadcastMessage(await node.GetFingerprint(context.CancellationToken));
                        await BroadcastProviderResolver.MemCached.Publish(msg, context.CancellationToken);
                        docChangedCount++;
                    }
                }
                scanCount++;
            }
            catch (Exception e)
            {
                exceptions.Add(e);
            }
        }
        context.OutputLog.Add($"Scanned {scanCount} (errors: {exceptions.Count}), of which {docChangedCount} had changes and {uptodateCount} did not");

        if (exceptions.Any())
        {
            throw new AggregateException(exceptions);
        }

        context.OutputLog.Add("Done!");
    }
}