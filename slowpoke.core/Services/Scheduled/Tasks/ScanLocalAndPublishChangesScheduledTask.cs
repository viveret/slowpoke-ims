using Microsoft.Extensions.DependencyInjection;
using slowpoke.core.Models.Broadcast.Messages;
using slowpoke.core.Models.Config;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services.Broadcast;
using slowpoke.core.Services.Node.Docs;

namespace slowpoke.core.Services.Scheduled.Tasks;


public class ScanLocalAndPublishChangesScheduledTask : IScheduledTask
{
    public Config Config { get; }
    public ISyncStateManager SyncStateManager { get; }
    public IDocumentProviderResolver DocumentProviderResolver { get; }
    public IServiceProvider Services { get; }

    public string Title => "Scan local and publish changes";

    public bool CanRunConcurrently => false;
    
    public bool CanRunManually => true;

    public ScanLocalAndPublishChangesScheduledTask(
        Config config,
        ISyncStateManager syncStateManager,
        IDocumentProviderResolver documentProviderResolver,
        IServiceProvider services)
    {
        Config = config;
        SyncStateManager = syncStateManager;
        DocumentProviderResolver = documentProviderResolver;
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
        var query = new QueryDocumentOptions { SyncEnabled = true, Recursive = true, Path = Config.Paths.HomePath.AsIDocPath(Config) };
        var localNodesCount = DocumentProviderResolver.ReadLocal.GetCountOfNodes(query, context.CancellationToken);
        var localNodes = DocumentProviderResolver.ReadLocal.GetNodes(query, context.CancellationToken);
        context.OutputLog.Add($"Scanning for changes in {localNodesCount} local nodes that have enabled sync");

        var broadcasterProviderResolver = Services.GetRequiredService<IBroadcastProviderResolver>();

        int scanCount = 0, uptodateCount = 0, docChangedCount = 0, docMetaChangedCount = 0;
        var exceptions = new List<Exception>();
        foreach (var node in localNodes)
        {
            try
            {
                var meta = node.HasMeta ? node.Meta : null;
                if (meta != null && meta.MetaExists)
                {
                    if (meta.LastSyncDate.HasValue)
                    {
                        if (meta.LastSyncDate < meta.LastUpdate)
                        {
                            var msg = new DocumentChangedBroadcastMessage(node.GetFingerprint(context.CancellationToken));
                            broadcasterProviderResolver.MemCached.Publish(msg, context.CancellationToken);
                            docChangedCount++;
                        }
                        else if (meta.LastSyncDate < meta.LastUpdate)
                        {
                            var msg = new DocMetaChangedBroacastMessage(node.GetFingerprint(context.CancellationToken));
                            broadcasterProviderResolver.MemCached.Publish(msg, context.CancellationToken);
                            docMetaChangedCount++;
                        }
                        else
                        {
                            uptodateCount++;
                        }
                    }
                    else
                    {
                        var msg = new SyncStartedBroadcastMessage(node.GetFingerprint(context.CancellationToken));
                        broadcasterProviderResolver.MemCached.Publish(msg, context.CancellationToken);
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
        return Task.CompletedTask;
    }
}