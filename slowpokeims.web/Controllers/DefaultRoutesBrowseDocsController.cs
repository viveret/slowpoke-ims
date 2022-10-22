using System;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using slowpoke.core.Models.Config;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services;
using slowpoke.core.Services.Node;
using slowpoke.core.Services.Node.Docs;
using SlowPokeIMS.Web.ViewModels;

namespace SlowPokeIMS.Web.Controllers;


public abstract class DefaultRoutesBrowseDocsController<TIndexViewModel, TDetailsViewModel> : BrowseDocsControllerBase
where TIndexViewModel: QueryDocumentsResult, new()
where TDetailsViewModel: SingleDocumentResult, new()
{
    public DefaultRoutesBrowseDocsController(
        IDocumentProviderResolver documentResolver,
        IUserSpecialFoldersProvider userSpecialFoldersProvider,
        Config config): base(documentResolver, userSpecialFoldersProvider, config)
    {
    }

    protected override PaginatedEnumerableViewModel<IReadOnlyNodeViewModel> Search(QueryDocumentOptions q, CancellationToken cancellationToken)
    {
        ForceControllerSpecificOptions(q);
        q.PageSize = GetPageSizeForViewType(q.ItemView);
        return base.Search(q, cancellationToken);
    }

    protected abstract void ForceControllerSpecificOptions(QueryDocumentOptions q);

    protected ActionResult Index(QueryDocumentOptions q, CancellationToken cancellationToken)
    {
        return GetActionResultForSearch(q, cancellationToken);
    }

    protected ActionResult GetActionResultForSearch(QueryDocumentOptions q, CancellationToken cancellationToken)
    {
        return View(new TIndexViewModel
        {
            q = q,
            Documents = Search(q, cancellationToken),
        });
    }

    protected ActionResult SearchTypedNotes(QueryDocumentOptions q, string contentType, CancellationToken cancellationToken)
    {
        q.ContentType = contentType;
        return GetActionResultForSearch(q, cancellationToken);
    }

    protected ActionResult TextNotes(QueryDocumentOptions q, CancellationToken cancellationToken) => SearchTypedNotes(q, "text", cancellationToken);
    protected ActionResult ImageNotes(QueryDocumentOptions q, CancellationToken cancellationToken) => SearchTypedNotes(q, "image", cancellationToken);
    protected ActionResult AudioNotes(QueryDocumentOptions q, CancellationToken cancellationToken) => SearchTypedNotes(q, "audio", cancellationToken);
    protected ActionResult VideoNotes(QueryDocumentOptions q, CancellationToken cancellationToken) => SearchTypedNotes(q, "video", cancellationToken);

    protected ActionResult Details(string path, CancellationToken cancellationToken)
    {
        var doc = this.ValidateAndResolvePath(path, cancellationToken);
        if (doc.Item3 != null) return doc.Item3;
        var vm = new TDetailsViewModel { };
        vm.Document = new IReadOnlyNodeViewModel(DocumentResolver.UnifiedReadable.CanRead.GetNodeAtPath(doc.Item1.AsIDocPath(Config), cancellationToken), null);
        return View(vm);
    }

    protected ActionResult Raw(string path, CancellationToken cancellationToken)
    {
        var doc = this.ValidateAndResolvePath(path, cancellationToken);
        if (doc.Item3 != null) return doc.Item3;
        
        var meta = doc.Item2.Meta;
        return File((doc.Item2 as IReadOnlyDocument).OpenRead(), meta.ContentType, meta.LastUpdate, Microsoft.Net.Http.Headers.EntityTagHeaderValue.Any, true);
    }
}