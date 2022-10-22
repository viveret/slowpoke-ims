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
    public void WriteIfChanged(Action<Stream> writer, CancellationToken cancellationToken)
    {
        var old = new ReadOnlyDocument(DocumentResolver, BroadcastProvider, Path);
        var streamDiff = new StreamDiff(writer, old.OpenRead(), cancellationToken);
        
        var diff = old != null ? new DocumentDiff(old, this) : null;

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
            WriteMeta(cancellationToken);
        }
    }

    public void WriteMeta(CancellationToken cancellationToken)
    {
        var meta = GetWritableMeta(cancellationToken);
        meta.WriteIfChanged(touch: true, cancellationToken);
    }

    public override IReadOnlyDocumentMeta Meta
    {
        get => (IReadOnlyDocumentMeta)writableMeta ?? base.Meta;
    }

    public IWritableDocumentMeta GetWritableMeta(CancellationToken cancellationToken)
    {
        return writableMeta ??= new WritableDocumentMeta(DocumentResolver, this.Path);
    }

    public override void TurnOnSync(CancellationToken cancellationToken)
    {
        BroadcastProvider.Publish(new SyncStartedBroadcastMessage(GetFingerprint(cancellationToken) as NodeFingerprintModel), cancellationToken);

        var meta = GetWritableMeta(cancellationToken);
        meta.SyncEnabled = true;
        meta.WriteIfChanged(cancellationToken: cancellationToken);
        BroadcastChanges(cancellationToken);
        // base.TurnOnSync(cancellationToken);
    }

    public override void TurnOffSync(CancellationToken cancellationToken)
    {
        var meta = GetWritableMeta(cancellationToken);
        meta.SyncEnabled = false;
        meta.WriteIfChanged(cancellationToken: cancellationToken);
        BroadcastChanges(cancellationToken);

        BroadcastProvider.Publish(new SyncStoppedBroadcastMessage(GetFingerprint(cancellationToken) as NodeFingerprintModel), cancellationToken);
        // base.TurnOffSync(cancellationToken);
    }

    public override void BroadcastChanges(CancellationToken cancellationToken)
    {
        BroadcastProvider.Publish(new DocumentChangedBroadcastMessage(GetFingerprint(cancellationToken) as NodeFingerprintModel), cancellationToken);
    }

    public override IEnumerable<INodeDiffBrief> FetchChanges(CancellationToken cancellationToken)
    {
        BroadcastProvider.Receive(Guid.Empty, cancellationToken);
        return Enumerable.Empty<INodeDiffBrief>();
    }

    public override void MergeChanges(CancellationToken cancellationToken)
    {
        // BroadcastProvider.Publish(new ChangesMessage(), cancellationToken);
    }

    public override void PollForChanges(CancellationToken cancellationToken)
    {
    }

    public override void Sync(CancellationToken cancellationToken)
    {
    }
}