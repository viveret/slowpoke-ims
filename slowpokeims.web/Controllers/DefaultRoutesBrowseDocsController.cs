using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services.Node.Docs;
using SlowPokeIMS.Web.ViewModels;

namespace SlowPokeIMS.Web.Controllers;


public abstract class DefaultRoutesBrowseDocsController<TIndexViewModel, TDetailsViewModel> : BrowseDocsControllerBase
where TIndexViewModel: QueryDocumentsResult, new()
where TDetailsViewModel: SingleDocumentResult, new()
{
    public DefaultRoutesBrowseDocsController(
        IDocumentProviderResolver documentResolver,
        //IUserSpecialFoldersProvider userSpecialFoldersProvider,
        Config config): base(documentResolver/*, userSpecialFoldersProvider*/, config)
    {
    }

    protected override async Task<PaginatedEnumerableViewModel<IReadOnlyNodeViewModel>> Search(QueryDocumentOptions q, CancellationToken cancellationToken)
    {
        ForceControllerSpecificOptions(q);
        q.PageSize = GetPageSizeForViewType(q.ItemView);
        return await base.Search(q, cancellationToken);
    }

    protected abstract void ForceControllerSpecificOptions(QueryDocumentOptions q);

    protected Task<ActionResult> Index(QueryDocumentOptions q, CancellationToken cancellationToken)
    {
        return GetActionResultForSearch(q, cancellationToken);
    }

    protected async Task<ActionResult> GetActionResultForSearch(QueryDocumentOptions q, CancellationToken cancellationToken)
    {
        return View(new TIndexViewModel
        {
            q = q,
            Documents = await Search(q, cancellationToken),
        });
    }

    protected async Task<ActionResult> SearchTypedNotes(QueryDocumentOptions q, string contentType, CancellationToken cancellationToken)
    {
        q.ContentType = contentType;
        return await GetActionResultForSearch(q, cancellationToken);
    }

    protected Task<ActionResult> TextNotes(QueryDocumentOptions q, CancellationToken cancellationToken) => SearchTypedNotes(q, "text", cancellationToken);
    protected Task<ActionResult> ImageNotes(QueryDocumentOptions q, CancellationToken cancellationToken) => SearchTypedNotes(q, "image", cancellationToken);
    protected Task<ActionResult> AudioNotes(QueryDocumentOptions q, CancellationToken cancellationToken) => SearchTypedNotes(q, "audio", cancellationToken);
    protected Task<ActionResult> VideoNotes(QueryDocumentOptions q, CancellationToken cancellationToken) => SearchTypedNotes(q, "video", cancellationToken);

    protected async Task<ActionResult> Details(string path, CancellationToken cancellationToken)
    {
        var doc = await this.ValidateAndResolvePath(path, cancellationToken);
        if (doc.Item3 != null) return doc.Item3;
        var vm = new TDetailsViewModel { };
        vm.Document = new IReadOnlyNodeViewModel(await (await DocumentResolver.UnifiedReadable).CanRead.GetNodeAtPath(doc.Item1.AsIDocPath(Config), cancellationToken), null);
        return View("Details", vm);
    }

    protected async Task<ActionResult> Raw(string path, CancellationToken cancellationToken)
    {
        var doc = await this.ValidateAndResolvePath(path, cancellationToken);
        if (doc.Item3 != null) return doc.Item3;
        
        var meta = await doc.Item2.Meta;
        return File(await (doc.Item2 as IReadOnlyDocument).OpenRead(), await meta.ContentType, meta.LastUpdate, Microsoft.Net.Http.Headers.EntityTagHeaderValue.Any, true);
    }
}