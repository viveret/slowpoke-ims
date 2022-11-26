using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Models.Node.Docs.ReadOnlyRemote;
using slowpoke.core.Services.Node.Docs;

namespace SlowPokeIMS.Web.Controllers.Api;


public partial class ApiController
{
    [HttpGet("api/total-count-of-documents")]
    public async Task<ActionResult> GetCountOfNodes(CancellationToken cancellationToken) => new JsonResult(await (await DocumentResolver.ReadLocal).GetCountOfNodes(cancellationToken));
    
    [HttpGet("api/count-of-documents")]
    public async Task<ActionResult> GetCountOfDocuments() => new JsonResult((await DocumentResolver.ReadLocal).Permissions);
    
    [HttpGet("api/count-of-documents-in-folder/{path}")]
    public async Task<ActionResult> GetCountOfNodesInFolder(string path) => new JsonResult((await DocumentResolver.ReadLocal).ResolverTypeName);
    
    [HttpGet("api/get-documents-in-folder/{path}")]
    public async Task<ActionResult> GetDocumentsInFolder(string path) => new JsonResult((await DocumentResolver.ReadLocal).ResolverTypeName);
    
    [HttpGet("api/get-document/{path}")]
    public async Task<ActionResult> GetDocument(string path)
    {
        var rl = await DocumentResolver.ReadLocal;
        var docPath = Uri.UnescapeDataString(path).AsIDocPath(Config);
        var node = await (rl).GetNodeAtPath(docPath, CancellationToken.None);
        if (node is IReadOnlyDocument readOnlyDocument)
        {
            return new JsonResult(await JsonReadOnlyDocument.CreateDoc(readOnlyDocument));
        }
        else
        {
            return new JsonResult(await JsonReadOnlyNode.CreateNode(node));
        }
    }

    // [HttpGet("api/get-docs-in-folder/{path}")]
    // public Task<ActionResult> GetLeastRecentlyCreatedDocuments(int offset, int amount, CancellationToken cancellationToken) =>
    //     new JsonResult(DocumentResolver.ReadLocal.GetNodesInFolder().OfType<IReadOnlyDocument>(), cancellationToken);

    // [HttpGet("api/get-docs-in-folder/{path}")]
    // public Task<ActionResult> GetLeastRecentlyUpdatedDocuments(int offset, int amount, CancellationToken cancellationToken) =>
    //     slowPokeClient.Search<IEnumerable<IReadOnlyDocument>>($"api/least-recently-updated-docs?Offset={offset}&PageSize={amount}", null, null, json => ParseDocuments(json), cancellationToken);

    [HttpGet("api/get-meta/{path}")]
    public async Task<ActionResult> GetMeta(IReadOnlyNode node, CancellationToken cancellationToken) =>
        new JsonResult(await (await DocumentResolver.ReadLocal).GetMeta(node, cancellationToken));

    // [HttpGet("api/get-docs-in-folder/{path}")]
    // public ActionResult GetMostRecentlyCreatedDocuments(int offset, int amount, CancellationToken cancellationToken) => 
    //     slowPokeClient.Search<IEnumerable<IReadOnlyDocument>>($"api/most-recently-created-docs?Offset={offset}&PageSize={amount}", null, null, json => ParseDocuments(json), cancellationToken);

    // [HttpGet("api/get-docs-in-folder/{path}")]
    // public ActionResult GetMostRecentlyUpdatedDocuments(int offset, int amount, CancellationToken cancellationToken) =>
    //     slowPokeClient.Search<IEnumerable<IReadOnlyDocument>>($"api/most-recently-updated-docs?Offset={offset}&PageSize={amount}", null, null, json => ParseDocuments(json), cancellationToken);

    [HttpGet("api/get-nodes/{path}")]
    public async Task<ActionResult> GetNodes(QueryDocumentOptions options, CancellationToken cancellationToken) =>
        new JsonResult(await (await DocumentResolver.ReadLocal).GetNodes(options, cancellationToken));

    [HttpGet("api/get-paths/{path}")]
    public async Task<ActionResult> GetPaths(QueryDocumentOptions options, CancellationToken cancellationToken) =>
        new JsonResult(await (await DocumentResolver.ReadLocal).GetPaths(options, cancellationToken), cancellationToken);

    [HttpGet("api/has-meta/{path}")]
    public async Task<ActionResult> HasMeta(IReadOnlyNode node, CancellationToken cancellationToken) =>
        new JsonResult(await (await DocumentResolver.ReadLocal).HasMeta(node, cancellationToken));

    [HttpGet("api/get-count-of-paths/{path}")]
    public async Task<ActionResult> GetCountOfPaths(QueryDocumentOptions options, CancellationToken cancellationToken) =>
        new JsonResult(await (await DocumentResolver.ReadLocal).GetCountOfPaths(options, cancellationToken));

    [HttpGet("api/get-fingerprints/{path}")]
    public async Task<ActionResult> FetchFingerprintsForNode(string path, CancellationToken cancellationToken)
    {
        var node = await (await DocumentResolver.ReadLocal).GetNodeAtPath(path.AsIDocPath(Config), cancellationToken);
        if (node == null || !await node.Exists)
        {
            return NotFound();
        }
        return new JsonResult(await (await DocumentResolver.ReadLocal).FetchFingerprintsForNode(node, cancellationToken));
    }
}