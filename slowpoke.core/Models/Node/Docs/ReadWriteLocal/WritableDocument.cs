using slowpoke.core.Models.Broadcast.Messages;
using slowpoke.core.Models.Diff;
using slowpoke.core.Models.Node.Docs.ReadOnlyLocal;
using slowpoke.core.Services.Broadcast;
using slowpoke.core.Services.Node.Docs;
using slowpoke.core.Util;

namespace slowpoke.core.Models.Node.Docs.ReadWriteLocal;


public class WritableDocument : ReadOnlyDocument, IWritableDocument
{
    private IWritableDocumentMeta writableMeta;

    public WritableLocalDocumentResolver DocumentResolver { get; }

    public WritableDocument(
        WritableLocalDocumentResolver documentResolver,
        IBroadcastProvider broadcastProvider, INodePath path): base(documentResolver, broadcastProvider, path)
    {
        DocumentResolver = documentResolver;
    }

    // todo: fix this, if client calls with using() then the stream will be closed prematurely and cause problems
    public async Task WriteIfChanged(Action<Stream> writer, CancellationToken cancellationToken)
    {
        var old = new ReadOnlyDocument(DocumentResolver, BroadcastProvider, Path);
        // var streamDiff = new StreamDiff(writer, await old.OpenRead(), true, true, cancellationToken);
        
        var diff = old != null ? await DocumentDiff.Create(old, this, true, cancellationToken) : null;

        if (diff == null || diff.HasChanged)
        {
            // save to local file
            var path = this.Path.ConvertToAbsolutePath().PathValue;
            var parentPath = System.IO.Path.GetDirectoryName(path);
            if (parentPath != null && !Directory.Exists(parentPath))
            {
                Directory.CreateDirectory(parentPath);
            }

            // file is being used by another process - this is broken
            using (var streamDestination = File.OpenWrite(path))
            {
                writer(streamDestination);
            }
            
            // save meta to local file            
            await WriteMeta(cancellationToken);
        }
    }

    public async Task WriteMeta(CancellationToken cancellationToken)
    {
        var meta = await GetWritableMeta(cancellationToken);
        await meta.WriteIfChanged(touch: true, cancellationToken);
    }

    public override Task<IReadOnlyDocumentMeta> Meta
    {
        get => writableMeta != null ? Task.FromResult<IReadOnlyDocumentMeta>(writableMeta) : base.Meta;
    }

    public Task<IWritableDocumentMeta> GetWritableMeta(CancellationToken cancellationToken)
    {
        return Task.FromResult<IWritableDocumentMeta>(writableMeta ??= new WritableDocumentMeta(DocumentResolver, this.Path));
    }

    public override async Task TurnOnSync(CancellationToken cancellationToken)
    {
        await BroadcastProvider.Publish(new SyncStartedBroadcastMessage((NodeFingerprintModel) await GetFingerprint(cancellationToken)), cancellationToken);

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

        await BroadcastProvider.Publish(new SyncStoppedBroadcastMessage((NodeFingerprintModel) await GetFingerprint(cancellationToken)), cancellationToken);
        // base.TurnOffSync(cancellationToken);
    }

    public override async Task BroadcastChanges(CancellationToken cancellationToken)
    {
        await BroadcastProvider.Publish(new DocumentChangedBroadcastMessage((NodeFingerprintModel) await GetFingerprint(cancellationToken)), cancellationToken);
    }

    public override async Task<IEnumerable<INodeDiffBrief>> FetchChanges(CancellationToken cancellationToken)
    {
        await BroadcastProvider.Receive(Guid.Empty, cancellationToken);
        return Enumerable.Empty<INodeDiffBrief>();
    }

    public override Task MergeChanges(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
        // BroadcastProvider.Publish(new ChangesMessage(), cancellationToken);
    }

    public override Task PollForChanges(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public override Task Sync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}