using System.Threading;
using slowpoke.core.Client;
using slowpoke.core.Models.Config;
using slowpoke.core.Models.Diff;
using slowpoke.core.Models.Node;
using slowpoke.core.Models.Node.Docs;

namespace slowpoke.core.Services.Node.Docs;



public class HttpReadOnlyRemoteDocumentResolver: AbstractReadOnlyRemoteDocumentResolverBase
{
    protected readonly ISlowPokeClient slowPokeClient;

    public HttpReadOnlyRemoteDocumentResolver(ISlowPokeClient client, IDocumentProviderResolver providerResolver, Config config): base(client.Endpoint, providerResolver, config)
    {
        slowPokeClient = client;
    }

    // needs to be parser added to services via dependency injection
    private IReadOnlyDocument ParseDocument(string json)
    {
        throw new NotImplementedException();
    }

    // needs to be parser added to services via dependency injection
    private IReadOnlyDocumentMeta ParseDocumentMeta(string json)
    {
        throw new NotImplementedException();
    }

    // needs to be parser added to services via dependency injection
    private IReadOnlyNode ParseNode(string json)
    {
        throw new NotImplementedException();
    }
    
    // needs to be parser added to services via dependency injection
    private IEnumerable<IReadOnlyDocument> ParseDocuments(string json)
    {
        throw new NotImplementedException();
    }
    
    // needs to be parser added to services via dependency injection
    private IEnumerable<IReadOnlyNode> ParseNodes(string json)
    {
        throw new NotImplementedException();
    }

    // needs to be parser added to services via dependency injection
    private IEnumerable<INodePath> ParsePaths(string json)
    {
        throw new NotImplementedException();
    }

    public NodePermissionCategories<bool> Permissions
    {
        get
        {
            var perms = base.Permissions;
            perms.IsEncrypted = Endpoint.Scheme == "https";
            return perms;
        }
    }

    public override bool NodeExistsAtPath(INodePath path, CancellationToken cancellationToken)
    {
        return slowPokeClient.NodeExistsAtPath(path, cancellationToken);
    }

    public override string GetContentTypeFromExtension(string extension)
    {
        throw new NotImplementedException();
    }

    public override int GetCountOfNodes(CancellationToken cancellationToken) =>
        slowPokeClient.SearchCount("api/total-count-of-documents", null, cancellationToken);

    public override int GetCountOfDocuments(QueryDocumentOptions options, CancellationToken cancellationToken) =>
        slowPokeClient.SearchCount("api/count-of-documents", options, cancellationToken);

    public override int GetCountOfNodesInFolder(INodePath folder, CancellationToken cancellationToken) =>
        slowPokeClient.SearchCount($"api/count-of-documents-in-folder/{Uri.EscapeDataString(folder.PathValue)}", null, cancellationToken);

    public override int GetCountOfNodes(QueryDocumentOptions options, CancellationToken cancellationToken) =>
        slowPokeClient.SearchCount("api/count-of-nodes", options, cancellationToken);

    public override IReadOnlyDocument GetNodeAtPath(INodePath path, CancellationToken cancellationToken) =>
        slowPokeClient.Query($"api/get-document/{Uri.EscapeDataString(path.PathValue)}",
            request => request.Headers.Add("Accept", "application/json"), json => ParseDocument(json), cancellationToken);

    public override IEnumerable<IReadOnlyDocument> GetDocuments(QueryDocumentOptions options, CancellationToken cancellationToken) =>
        slowPokeClient.Search("api/get-documents", options, null, responseHandler: json => ParseDocuments(json), cancellationToken);

    public override IEnumerable<IReadOnlyNode> GetNodesInFolder(INodePath folder, int offset, int amount, CancellationToken cancellationToken) =>
        slowPokeClient.Search<IEnumerable<IReadOnlyDocument>>($"api/get-docs-in-folder/{Uri.EscapeDataString(folder.PathValue)}?Offset={offset}&PageSize={amount}", null, null, json => ParseDocuments(json), cancellationToken);

    public override string GetExtensionFromContentType(string contentType)
    {
        throw new NotImplementedException();
    }

    // public override IEnumerable<IReadOnlyDocument> GetLeastRecentlyCreatedDocuments(int offset, int amount, CancellationToken cancellationToken) =>
    //     slowPokeClient.Search<IEnumerable<IReadOnlyDocument>>($"api/least-recently-created-docs?Offset={offset}&PageSize={amount}", null, null, json => ParseDocuments(json), cancellationToken);

    // public override IEnumerable<IReadOnlyDocument> GetLeastRecentlyUpdatedDocuments(int offset, int amount, CancellationToken cancellationToken) =>
    //     slowPokeClient.Search<IEnumerable<IReadOnlyDocument>>($"api/least-recently-updated-docs?Offset={offset}&PageSize={amount}", null, null, json => ParseDocuments(json), cancellationToken);

    public override IReadOnlyDocumentMeta GetMeta(IReadOnlyNode node, CancellationToken cancellationToken) =>
        slowPokeClient.GetMeta(node, cancellationToken);

    // public override IEnumerable<IReadOnlyDocument> GetMostRecentlyCreatedDocuments(int offset, int amount, CancellationToken cancellationToken) => 
    //     slowPokeClient.Search<IEnumerable<IReadOnlyDocument>>($"api/most-recently-created-docs?Offset={offset}&PageSize={amount}", null, null, json => ParseDocuments(json), cancellationToken);

    // public override IEnumerable<IReadOnlyDocument> GetMostRecentlyUpdatedDocuments(int offset, int amount, CancellationToken cancellationToken) =>
    //     slowPokeClient.Search<IEnumerable<IReadOnlyDocument>>($"api/most-recently-updated-docs?Offset={offset}&PageSize={amount}", null, null, json => ParseDocuments(json), cancellationToken);

    public override IEnumerable<IReadOnlyNode> GetNodes(QueryDocumentOptions options, CancellationToken cancellationToken) =>
        slowPokeClient.Search<IEnumerable<IReadOnlyNode>>("api/get-nodes", options, null, json => ParseNodes(json), cancellationToken);

    public override IEnumerable<INodePath> GetPaths(QueryDocumentOptions options, CancellationToken cancellationToken) => 
        slowPokeClient.Search<IEnumerable<INodePath>>("api/get-paths", options, null, json => ParsePaths(json), cancellationToken);

    public override bool HasMeta(IReadOnlyNode node, CancellationToken cancellationToken) => 
        slowPokeClient.HasMeta(node, cancellationToken);

    public override int GetCountOfPaths(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<INodeFingerprint> FetchFingerprintsForNode(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        return slowPokeClient.Search<IEnumerable<INodeFingerprint>>($"api/get-fingerprints/{Uri.EscapeDataString(node.Path.PathValue)}", null, null, json => ParseFingerprints(json), cancellationToken);
    }

    private IEnumerable<INodeFingerprint> ParseFingerprints(string json)
    {
        throw new NotImplementedException();
    }
}