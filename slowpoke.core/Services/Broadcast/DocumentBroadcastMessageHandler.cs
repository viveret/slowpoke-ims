using slowpoke.core.Models.Broadcast;
using slowpoke.core.Models.Broadcast.Messages;
using slowpoke.core.Models.Config;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services.Identity;
using slowpoke.core.Services.Node.Docs;

namespace slowpoke.core.Services.Broadcast;



public class DocumentBroadcastMessageHandler : IBroadcastMessageHandler
{
    public Config Config { get; }
    public IDocumentProviderResolver DocumentProviderResolver { get; }
    public IIdentityAuthenticationService IdentityAuthenticationService { get; }

    public DocumentBroadcastMessageHandler(
        Config config,
        IDocumentProviderResolver documentProviderResolver,
        IIdentityAuthenticationService identityAuthenticationService)
    {
        Config = config;
        DocumentProviderResolver = documentProviderResolver;
    }

    public Type[] MessageTypesAllowed { get; } = new Type[] {
        typeof(DocumentChangedBroadcastMessage), typeof(DocFingerprintBroacastMessage),
        typeof(SyncStartedBroadcastMessage), typeof(SyncStoppedBroadcastMessage),
    };

    public void Process(IBroadcastMessage message, CancellationToken cancellationToken)
    {
        var rwl = DocumentProviderResolver.ReadWriteLocal;
        if (message is DocumentChangedBroadcastMessage docChanged)
        {
            var p = docChanged.Fingerprint.Path.AsIDocPath(Config);
            if (rwl.NodeExistsAtPath(p, cancellationToken))
            {
                var endpoint = IdentityAuthenticationService.GetEndpointForOriginGuid(message.OriginGuid, cancellationToken);
                var nodeRemote = DocumentProviderResolver.OpenReadRemote(endpoint, null).GetNodeAtPath(p, cancellationToken);
                var node = (IWritableDocument) rwl.GetNodeAtPath(docChanged.Fingerprint.Path.AsIDocPath(Config), cancellationToken);
                using var nodeData = new MemoryStream();
                node.WriteIfChanged(stream => nodeData.CopyTo(stream), cancellationToken);
                var meta = node.GetWritableMeta(cancellationToken);
                // stream => nodeData.CopyTo(stream)
                meta.WriteIfChanged(cancellationToken: cancellationToken);
            }
            else
            {
                throw new Exception("Document does not exist, need to use DocumentCreatedBroadcastMessage");
            }
        }
        else if (message is DocMetaChangedBroacastMessage docMetaChanged)
        {
            if (rwl.NodeExistsAtPath(docMetaChanged.Fingerprint.Path.AsIDocPath(Config), cancellationToken))
            {
                var node = (IWritableDocument) rwl.GetNodeAtPath(docMetaChanged.Fingerprint.Path.AsIDocPath(Config), cancellationToken);
                using var nodeData = new MemoryStream();
                // stream => nodeData.CopyTo(stream)
                var meta = node.GetWritableMeta(cancellationToken);
                meta.SyncEnabled = false;
                // stream => nodeData.CopyTo(stream)
                meta.WriteIfChanged(cancellationToken: cancellationToken);
            }
            else
            {
                throw new Exception("Document does not exist, need to use DocumentCreatedBroadcastMessage");
            }
        }
        else if (message is SyncStartedBroadcastMessage syncStarted)
        {
            var node = (IWritableDocument) rwl.GetNodeAtPath(syncStarted.Fingerprint.Path.AsIDocPath(Config), cancellationToken);
            using var nodeData = new MemoryStream();
            // stream => nodeData.CopyTo(stream)
            var meta = node.GetWritableMeta(cancellationToken);
            meta.SyncEnabled = true;
            // stream => nodeData.CopyTo(stream)
            meta.WriteIfChanged(cancellationToken: cancellationToken);
        }
        else if (message is SyncStoppedBroadcastMessage syncStopped)
        {
            var node = (IWritableDocument) rwl.GetNodeAtPath(syncStopped.Fingerprint.Path.AsIDocPath(Config), cancellationToken);
            using var nodeData = new MemoryStream();
            var meta = node.GetWritableMeta(cancellationToken);
            meta.SyncEnabled = false;
            // stream => nodeData.CopyTo(stream)
            meta.WriteIfChanged(cancellationToken: cancellationToken);
        }
        else
        {
            throw new ArgumentOutOfRangeException();
        }
    }
}