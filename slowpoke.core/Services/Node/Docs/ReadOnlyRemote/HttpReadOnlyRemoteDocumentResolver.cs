using System.Threading;
using slowpoke.core.Client;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Diff;
using slowpoke.core.Models.Node;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Models.Node.Docs.ReadOnlyRemote;

namespace slowpoke.core.Services.Node.Docs;



public class HttpReadOnlyRemoteDocumentResolver: AbstractReadOnlyRemoteDocumentResolverBase
{
    protected readonly ISlowPokeClient slowPokeClient;

    public HttpReadOnlyRemoteDocumentResolver(
        ISlowPokeClient client, Config config,
        IDocumentProviderResolver providerResolver): base(client.Endpoint, providerResolver, config)
    {
        slowPokeClient = client;
    }

    // needs to be parser added to services via dependency injection
    private async Task<IReadOnlyDocument> ParseDocument(string json)
    {
        try
        {
            var model = System.Text.Json.JsonSerializer.Deserialize<JsonReadOnlyDocument>(json);
            var doc = await RemoteReadOnlyDocument.CreateDoc(model, this);
            if (model.MetaSynch.LastUpdate == null)
            {
                throw new Exception($"MetaSynch was null: {json}");
            }
            return doc;
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to parse json for {nameof(RemoteReadOnlyDocument)}: {json}", e);
        }
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
    private Task<IEnumerable<IReadOnlyDocument>> ParseDocuments(string json)
    {
        throw new NotImplementedException();
    }
    
    // needs to be parser added to services via dependency injection
    private Task<IEnumerable<IReadOnlyNode>> ParseNodes(string json)
    {
        throw new NotImplementedException();
    }

    // needs to be parser added to services via dependency injection
    private Task<IEnumerable<INodePath>> ParsePaths(string json)
    {
        throw new NotImplementedException();
    }

    public override Task<NodePermissionCategories<bool>> Permissions
    {
        get => GetAndExtendBasePermissions();
    }

    private async Task<NodePermissionCategories<bool>> GetAndExtendBasePermissions()
    {
        var perms = await base.Permissions;
        perms.IsEncrypted = Endpoint.Scheme == "https";
        return perms;
    }

    public override Task<bool> NodeExistsAtPath(INodePath path, CancellationToken cancellationToken)
    {
        return slowPokeClient.NodeExistsAtPath(path, cancellationToken);
    }

    public override Task<string> GetContentTypeFromExtension(string extension)
    {
        throw new NotImplementedException();
    }

    public override Task<int> GetCountOfNodes(CancellationToken cancellationToken) =>
        slowPokeClient.SearchCount("api/total-count-of-documents", null, cancellationToken);

    public override Task<int> GetCountOfDocuments(QueryDocumentOptions options, CancellationToken cancellationToken) =>
        slowPokeClient.SearchCount("api/count-of-documents", options, cancellationToken);

    public override Task<int> GetCountOfNodesInFolder(INodePath folder, CancellationToken cancellationToken) =>
        slowPokeClient.SearchCount($"api/count-of-documents-in-folder/{Uri.EscapeDataString(folder.PathValue)}", null, cancellationToken);

    public override Task<int> GetCountOfNodes(QueryDocumentOptions options, CancellationToken cancellationToken) =>
        slowPokeClient.SearchCount("api/count-of-nodes", options, cancellationToken);

    public override async Task<IReadOnlyNode> GetNodeAtPath(INodePath path, CancellationToken cancellationToken)
    {
        var model = await slowPokeClient.QueryJson<JsonReadOnlyDocument>($"api/get-document/{Uri.EscapeDataString(path.PathValue)}", cancellationToken);
        if (model.MetaSynch.LastUpdate == null)
        {
            throw new Exception($"MetaSynch was null");
        }
        var doc = await RemoteReadOnlyDocument.CreateDoc(model, this);
        return doc;
    }

    public override Task<IEnumerable<IReadOnlyDocument>> GetDocuments(QueryDocumentOptions options, CancellationToken cancellationToken) =>
        slowPokeClient.Search("api/get-documents", options, null, responseHandler: json => ParseDocuments(json), cancellationToken);

    public override Task<IEnumerable<IReadOnlyNode>> GetNodesInFolder(INodePath folder, int offset, int amount, CancellationToken cancellationToken) =>
        slowPokeClient.Search<IEnumerable<IReadOnlyNode>>($"api/get-docs-in-folder/{Uri.EscapeDataString(folder.PathValue)}?Offset={offset}&PageSize={amount}", null, null, json => ParseNodes(json), cancellationToken);

    public override Task<string> GetExtensionFromContentType(string contentType)
    {
        throw new NotImplementedException();
    }

    // public override IEnumerable<IReadOnlyDocument> GetLeastRecentlyCreatedDocuments(int offset, int amount, CancellationToken cancellationToken) =>
    //     slowPokeClient.Search<IEnumerable<IReadOnlyDocument>>($"api/least-recently-created-docs?Offset={offset}&PageSize={amount}", null, null, json => ParseDocuments(json), cancellationToken);

    // public override IEnumerable<IReadOnlyDocument> GetLeastRecentlyUpdatedDocuments(int offset, int amount, CancellationToken cancellationToken) =>
    //     slowPokeClient.Search<IEnumerable<IReadOnlyDocument>>($"api/least-recently-updated-docs?Offset={offset}&PageSize={amount}", null, null, json => ParseDocuments(json), cancellationToken);

    public override Task<IReadOnlyDocumentMeta> GetMeta(IReadOnlyNode node, CancellationToken cancellationToken) =>
        slowPokeClient.GetMeta(node, cancellationToken);

    // public override IEnumerable<IReadOnlyDocument> GetMostRecentlyCreatedDocuments(int offset, int amount, CancellationToken cancellationToken) => 
    //     slowPokeClient.Search<IEnumerable<IReadOnlyDocument>>($"api/most-recently-created-docs?Offset={offset}&PageSize={amount}", null, null, json => ParseDocuments(json), cancellationToken);

    // public override IEnumerable<IReadOnlyDocument> GetMostRecentlyUpdatedDocuments(int offset, int amount, CancellationToken cancellationToken) =>
    //     slowPokeClient.Search<IEnumerable<IReadOnlyDocument>>($"api/most-recently-updated-docs?Offset={offset}&PageSize={amount}", null, null, json => ParseDocuments(json), cancellationToken);

    public override Task<IEnumerable<IReadOnlyNode>> GetNodes(QueryDocumentOptions options, CancellationToken cancellationToken) =>
        slowPokeClient.Search<IEnumerable<IReadOnlyNode>>("api/get-nodes", options, null, json => ParseNodes(json), cancellationToken);

    public override Task<IEnumerable<INodePath>> GetPaths(QueryDocumentOptions options, CancellationToken cancellationToken) => 
        slowPokeClient.Search<IEnumerable<INodePath>>("api/get-paths", options, null, json => ParsePaths(json), cancellationToken);

    public override Task<bool> HasMeta(IReadOnlyNode node, CancellationToken cancellationToken) => 
        slowPokeClient.HasMeta(node, cancellationToken);

    public override Task<int> GetCountOfPaths(QueryDocumentOptions options, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override Task<IEnumerable<INodeFingerprint>> FetchFingerprintsForNode(IReadOnlyNode node, CancellationToken cancellationToken)
    {
        return slowPokeClient.Search<IEnumerable<INodeFingerprint>>($"api/get-fingerprints/{Uri.EscapeDataString(node.Path.PathValue)}", null, null, json => ParseFingerprints(json), cancellationToken);
    }

    private Task<IEnumerable<INodeFingerprint>> ParseFingerprints(string json)
    {
        throw new NotImplementedException();
    }
}