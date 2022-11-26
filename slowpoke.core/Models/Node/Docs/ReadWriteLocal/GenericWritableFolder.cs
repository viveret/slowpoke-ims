
using slowpoke.core.Models.Broadcast.Messages;
using slowpoke.core.Models.Diff;
using slowpoke.core.Models.Node.Docs.ReadOnlyLocal;
using slowpoke.core.Services.Broadcast;
using slowpoke.core.Services.Node.Docs;
using slowpoke.core.Util;

namespace slowpoke.core.Models.Node.Docs.ReadWriteLocal;


public class GenericWritableFolder : GenericReadOnlyFolder, IWritableFolder
{
    private IWritableDocumentMeta writableMeta;

    public GenericReadWriteDocumentResolver DocumentResolver { get; }

    public GenericWritableFolder(
        GenericReadWriteDocumentResolver documentResolver,
        IBroadcastProvider broadcastProvider, INodePath path): base(documentResolver, broadcastProvider, path)
    {
        DocumentResolver = documentResolver;
    }

    public async Task<IWritableDocumentMeta> GetWritableMeta(CancellationToken cancellationToken)
    {
        return (await documentResolver.GetMeta(this, cancellationToken)) as IWritableDocumentMeta;
    }

    public async Task WriteMeta(CancellationToken cancellationToken)
    {
        await (await this.GetWritableMeta(cancellationToken)).OpenWriteMetaOnDisk(cancellationToken);
    }

    public override async Task TurnOnSync(CancellationToken cancellationToken)
    {
        await BroadcastProvider.Publish(new SyncStartedBroadcastMessage(await GetFingerprint(cancellationToken) as NodeFingerprintModel), cancellationToken);

        var meta = await GetWritableMeta(cancellationToken);
        meta.SyncEnabled = true;
        await meta.WriteIfChanged(cancellationToken: cancellationToken);
        await BroadcastChanges(cancellationToken);
        // base.TurnOnSync(cancellationToken);
    }

    public override async Task TurnOffSync(CancellationToken cancellationToken)
    {
        var meta = await GetWritableMeta(cancellationToken);
        meta.SyncEnabled = false;
        await meta.WriteIfChanged(cancellationToken: cancellationToken);
        await BroadcastChanges(cancellationToken);

        await BroadcastProvider.Publish(new SyncStoppedBroadcastMessage(await GetFingerprint(cancellationToken) as NodeFingerprintModel), cancellationToken);
        // base.TurnOnSync(cancellationToken);
    }

    public override async Task BroadcastChanges(CancellationToken cancellationToken)
    {
        await BroadcastProvider.Publish(new DocumentChangedBroadcastMessage(await GetFingerprint(cancellationToken) as NodeFingerprintModel), cancellationToken);
    }

    public override Task<IEnumerable<INodeDiffBrief>> FetchChanges(CancellationToken cancellationToken)
    {
        return Task.FromResult(Enumerable.Empty<INodeDiffBrief>());
        // return base.FetchChanges(cancellationToken);
    }

    public override Task MergeChanges(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
        // throw new NotImplementedException();
        // base.MergeChanges(cancellationToken);
    }

    public override Task Sync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
        // throw new NotImplementedException();
        // base.Sync(cancellationToken);
    }

    public override Task PollForChanges(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
        // throw new NotImplementedException();
        // base.PollForChanges(cancellationToken);
    }
}