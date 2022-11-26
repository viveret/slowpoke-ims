using Microsoft.Extensions.DependencyInjection;
using slowpoke.core.Models.Broadcast.Messages;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services.Broadcast;
using slowpoke.core.Services.Node.Docs;
using SlowPokeIMS.Core.Services.Scheduled;

namespace slowpoke.core.Services.Scheduled.Tasks;


public class ScanLocalAndPullChangesScheduledTask : ScheduledTaskBase
{
    public IDocumentProviderResolver DocumentProviderResolver { get; }

    public override string Title => "Scan local and pull changes";

    public override bool CanRunConcurrently => false;
    
    public override bool CanRunManually => true;

    public ScanLocalAndPullChangesScheduledTask(
        Config config,
        ISyncStateManager syncStateManager,
        IServiceProvider services,
        IDocumentProviderResolver documentProviderResolver): base(config, syncStateManager, services)
    {
        DocumentProviderResolver = documentProviderResolver;
    }

    public override async Task Execute(IScheduledTaskContext context)
    {
        var remotes = await DocumentProviderResolver.ReadRemotes;
        if (!remotes.Any())
        {
            throw new Exception("No remotes, cannot sync");
        }

        // todo: need to update context such that onComplete returns true and error is set if exception is thrown.
        // executer class cannot do that because this task then depends on the executer to run
        SyncStateManager.SetForSystem(new Models.SyncState.SyncStateModel { LastTimePolledForChanges = DateTime.UtcNow, HasSentPublishedChanges = true, IsUpToDateWithPolledChanges = true, HasChangesToSend = true, Progress = new Models.SyncState.SyncStateTaskProgress { ProgressValue = 1, ProgressMax = 100 } });
        
        var query = new QueryDocumentOptions { SyncEnabled = true, Recursive = true, Path = Config.Paths.HomePath.AsIDocPath(Config) };
        var rl = await DocumentProviderResolver.ReadLocal;
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
                    // need to ensure this does not get the local version and that it gets the most recent among peers (might have to scan all peers)
                    var nodes = await Task.WhenAll(remotes.Select(r => r.GetNodeAtPath(node.Path, context.CancellationToken)));
                    var mostRecentVersionsFromPeers = (await Task.WhenAll(nodes.Select(async n => (n, (await n.Meta).LastUpdate)))).OrderByDescending(v => v.Item2).Select(v => v.n).ToList();
                    var mostRecentVersion = mostRecentVersionsFromPeers.FirstOrDefault();
                    IReadOnlyDocumentMeta? mostRecentMeta = null;
                    
                    if (mostRecentVersion != null)
                    {
                        mostRecentMeta = await mostRecentVersion.Meta;
                    }

                    var existsTask = mostRecentVersion?.Exists;
                    if (mostRecentVersion != null && existsTask == null)
                    {
                        throw new Exception($"Expected Task from {mostRecentVersion.GetType().FullName}.{nameof(mostRecentVersion.Exists)}, received null");
                    }

                    if (mostRecentVersion == null || (existsTask != null && !await existsTask))
                    {
                        uptodateCount++;
                    }
                    else if (meta.LastSyncDate.HasValue)
                    {
                        if (meta.LastSyncDate < mostRecentMeta!.LastUpdate)
                        {
                            docChangedCount++;
                        }
                        else if (meta.LastSyncDate < meta.LastUpdate)
                        {
                            docMetaChangedCount++;
                        }
                        else
                        {
                            uptodateCount++;
                        }
                    }
                    else if (meta.LastUpdate < mostRecentMeta!.LastUpdate)
                    {
                        docChangedCount++;
                    }
                    else if (meta.LastMetaUpdate < mostRecentMeta.LastMetaUpdate)
                    {
                        docChangedCount++;
                    }
                    else
                    {
                        uptodateCount++;
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
            if (exceptions.Count > 1)
            {
                throw new AggregateException(exceptions);
            }
            else
            {
                throw new Exception("error while synchronizing", exceptions.First());
            }
        }

        context.OutputLog.Add("Done!");
        SyncStateManager.SetForSystem(new Models.SyncState.SyncStateModel
        {
            LastTimePolledForChanges = DateTime.UtcNow,
            HasSentPublishedChanges = true,
            IsUpToDateWithPolledChanges = true,
            HasChangesToSend = true,
            Progress = null,
            LastTimeSentPublishedChanges = DateTime.UtcNow,
            LastTimeStateChanged = DateTime.UtcNow.AddSeconds(-10)
        });
    }
}