using slowpoke.core.Models.Broadcast;
using slowpoke.core.Models.Broadcast.Messages;
using slowpoke.core.Models.Configuration;
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

    public async Task Process(IBroadcastMessage message, CancellationToken cancellationToken)
    {
        var rwl = await DocumentProviderResolver.ReadWriteLocal;
        if (message is DocumentChangedBroadcastMessage docChanged)
        {
            var p = docChanged.Fingerprint!.Path.AsIDocPath(Config);
            if (await rwl.NodeExistsAtPath(p, cancellationToken))
            {
                var endpoint = IdentityAuthenticationService.GetEndpointForOriginGuid(message.OriginGuid, cancellationToken);
                var nodeRemote = (await DocumentProviderResolver.OpenReadRemote(endpoint, null)).GetNodeAtPath(p, cancellationToken);
                var node = (IWritableDocument) rwl.GetNodeAtPath(docChanged.Fingerprint.Path.AsIDocPath(Config), cancellationToken);
                using var nodeData = new MemoryStream();
                await node.WriteIfChanged(stream => nodeData.CopyTo(stream), cancellationToken);
                var meta = await node.GetWritableMeta(cancellationToken);
                // stream => nodeData.CopyTo(stream)
                await meta.WriteIfChanged(cancellationToken: cancellationToken);
            }
            else
            {
                throw new Exception("Document does not exist, need to use DocumentCreatedBroadcastMessage");
            }
        }
        else if (message is DocMetaChangedBroacastMessage docMetaChanged)
        {
            if (await rwl.NodeExistsAtPath(docMetaChanged.Fingerprint!.Path.AsIDocPath(Config), cancellationToken))
            {
                var node = (IWritableDocument) await rwl.GetNodeAtPath(docMetaChanged.Fingerprint.Path.AsIDocPath(Config), cancellationToken);
                using var nodeData = new MemoryStream();
                // stream => nodeData.CopyTo(stream)
                var meta = await node.GetWritableMeta(cancellationToken);
                meta.SyncEnabled = false;
                // stream => nodeData.CopyTo(stream)
                await meta.WriteIfChanged(cancellationToken: cancellationToken);
            }
            else
            {
                throw new Exception("Document does not exist, need to use DocumentCreatedBroadcastMessage");
            }
        }
        else if (message is SyncStartedBroadcastMessage syncStarted)
        {
            var node = (IWritableDocument) await rwl.GetNodeAtPath(syncStarted.Fingerprint!.Path.AsIDocPath(Config), cancellationToken);
            using var nodeData = new MemoryStream();
            // stream => nodeData.CopyTo(stream)
            var meta = await node.GetWritableMeta(cancellationToken);
            meta.SyncEnabled = true;
            // stream => nodeData.CopyTo(stream)
            await meta.WriteIfChanged(cancellationToken: cancellationToken);
        }
        else if (message is SyncStoppedBroadcastMessage syncStopped)
        {
            var node = (IWritableDocument) rwl.GetNodeAtPath(syncStopped.Fingerprint!.Path.AsIDocPath(Config), cancellationToken);
            using var nodeData = new MemoryStream();
            var meta = await node.GetWritableMeta(cancellationToken);
            meta.SyncEnabled = false;
            // stream => nodeData.CopyTo(stream)
            await meta.WriteIfChanged(cancellationToken: cancellationToken);
        }
        else
        {
            throw new ArgumentOutOfRangeException();
        }
    }
}