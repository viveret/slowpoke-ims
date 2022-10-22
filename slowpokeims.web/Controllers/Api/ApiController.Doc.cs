using System.Threading;
using Microsoft.AspNetCore.Mvc;
using slowpoke.core.Models.Config;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services.Node.Docs;

namespace SlowPokeIMS.Web.Controllers.Api;


public partial class ApiController
{
    [HttpGet("api/total-count-of-documents")]
    public ActionResult GetCountOfNodes(CancellationToken cancellationToken) => new JsonResult(DocumentResolver.ReadLocal.GetCountOfNodes(cancellationToken));
    
    [HttpGet("api/count-of-documents")]
    public ActionResult GetCountOfDocuments() => new JsonResult(DocumentResolver.ReadLocal.Permissions);
    
    [HttpGet("api/count-of-documents-in-folder/{path}")]
    public ActionResult GetCountOfNodesInFolder(string path) => new JsonResult(DocumentResolver.ReadLocal.ResolverTypeName);
    
    [HttpGet("api/get-documents")]
    public ActionResult GetDocuments(string path) => new JsonResult(DocumentResolver.ReadLocal.ResolverTypeName);
    
    [HttpGet("api/get-docs-in-folder/{path}")]
    public ActionResult GetDocumentsInFolder(string path) => new JsonResult(DocumentResolver.ReadLocal.ResolverTypeName);

    // [HttpGet("api/get-docs-in-folder/{path}")]
    // public ActionResult GetLeastRecentlyCreatedDocuments(int offset, int amount, CancellationToken cancellationToken) =>
    //     new JsonResult(DocumentResolver.ReadLocal.GetNodesInFolder().OfType<IReadOnlyDocument>(), cancellationToken);

    // [HttpGet("api/get-docs-in-folder/{path}")]
    // public ActionResult GetLeastRecentlyUpdatedDocuments(int offset, int amount, CancellationToken cancellationToken) =>
    //     slowPokeClient.Search<IEnumerable<IReadOnlyDocument>>($"api/least-recently-updated-docs?Offset={offset}&PageSize={amount}", null, null, json => ParseDocuments(json), cancellationToken);

    [HttpGet("api/get-meta/{path}")]
    public ActionResult GetMeta(IReadOnlyNode node, CancellationToken cancellationToken) =>
        new JsonResult(DocumentResolver.ReadLocal.GetMeta(node, cancellationToken));

    // [HttpGet("api/get-docs-in-folder/{path}")]
    // public ActionResult GetMostRecentlyCreatedDocuments(int offset, int amount, CancellationToken cancellationToken) => 
    //     slowPokeClient.Search<IEnumerable<IReadOnlyDocument>>($"api/most-recently-created-docs?Offset={offset}&PageSize={amount}", null, null, json => ParseDocuments(json), cancellationToken);

    // [HttpGet("api/get-docs-in-folder/{path}")]
    // public ActionResult GetMostRecentlyUpdatedDocuments(int offset, int amount, CancellationToken cancellationToken) =>
    //     slowPokeClient.Search<IEnumerable<IReadOnlyDocument>>($"api/most-recently-updated-docs?Offset={offset}&PageSize={amount}", null, null, json => ParseDocuments(json), cancellationToken);

    [HttpGet("api/get-nodes/{path}")]
    public ActionResult GetNodes(QueryDocumentOptions options, CancellationToken cancellationToken) =>
        new JsonResult(DocumentResolver.ReadLocal.GetNodes(options, cancellationToken));

    [HttpGet("api/get-paths/{path}")]
    public ActionResult GetPaths(QueryDocumentOptions options, CancellationToken cancellationToken) =>
        new JsonResult(DocumentResolver.ReadLocal.GetPaths(options, cancellationToken), cancellationToken);

    [HttpGet("api/has-meta/{path}")]
    public ActionResult HasMeta(IReadOnlyNode node, CancellationToken cancellationToken) =>
        new JsonResult(DocumentResolver.ReadLocal.HasMeta(node, cancellationToken));

    [HttpGet("api/get-count-of-paths/{path}")]
    public ActionResult GetCountOfPaths(QueryDocumentOptions options, CancellationToken cancellationToken) =>
        new JsonResult(DocumentResolver.ReadLocal.GetCountOfPaths(options, cancellationToken));

    [HttpGet("api/get-fingerprints/{path}")]
    public ActionResult FetchFingerprintsForNode(string path, CancellationToken cancellationToken)
    {
        var node = DocumentResolver.ReadLocal.GetNodeAtPath(path.AsIDocPath(Config), cancellationToken);
        if (node == null || !node.Exists)
        {
            return NotFound();
        }
        return new JsonResult(DocumentResolver.ReadLocal.FetchFingerprintsForNode(node, cancellationToken));
    }
}