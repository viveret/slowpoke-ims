using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services.Node.Docs;
using SlowPokeIMS.Web.Controllers.Attributes;
using SlowPokeIMS.Web.ViewModels;
using SlowPokeIMS.Web.ViewModels.SpecialFolders;

namespace SlowPokeIMS.Web.Controllers;


[ShowInNavBar("Special Folders", showInHorizontal: false)]
[DocController]
public class SpecialFoldersController : DefaultRoutesBrowseDocsController<IndexViewModel, ViewModels.Home.DetailsViewModel>// BrowseDocsControllerBase
{
    public SpecialFoldersController(
        IDocumentProviderResolver documentResolver,
        //IUserSpecialFoldersProvider userSpecialFoldersProvider,
        Config config): base(documentResolver/*, userSpecialFoldersProvider*/, config)
    {
    }

    [ShowInNavBar("Archived", showInHorizontal: false), HttpGet("special/archived")]
    public async Task<ActionResult> Archived([FromQuery] QueryDocumentOptions q, CancellationToken cancellationToken)
    {
        q.Path ??= Config.Paths.HomePath.AsIDocPath(Config);
        q.Recursive = true;
        q.PageSize = GetPageSizeForViewType(q.ItemView);
        return View(new IndexViewModel
        {
            q = q,
            Documents = await Search(q, cancellationToken),
        });
    }

    [ShowInNavBar("Favorites", showInHorizontal: false), HttpGet("special/favorites")]
    public async Task<ActionResult> Favorites(CancellationToken cancellationToken)
    {
        var q = new QueryDocumentOptions
        {
            IsInFavorites = true,
            Recursive = true,
            Path = "~/".AsIDocPath(Config),
        };
        // var favoritePaths = UserSpecialFoldersProvider.Favorites;
        // var favorites = favoritePaths.Select(p => DocumentResolver.UnifiedReadable.CanRead.GetNodeAtPath(p, cancellationToken));
        return View(new IndexViewModel
        {
            q = q,
            Documents = await Search(q, cancellationToken),
        });
    }

    protected override string DefaultDocActionNameForTitleUrlResolver(IReadOnlyNodeViewModel m, QueryDocumentOptions q)
    {
        // todo: this should be dependent on the type of special folder
        return "FavoritesDetails";
    }

    [HttpGet("special/favorites/details/{path}")]
    public Task<ActionResult> FavoritesDetails(string path, CancellationToken cancellationToken) => base.Details(path, cancellationToken);

    [ShowInNavBar("Sync Enabled", showInHorizontal: false), HttpGet("special/sync-enabled")]
    public async Task<ActionResult> SyncEnabled(CancellationToken cancellationToken)
    {
        var q = new QueryDocumentOptions
        {
            SyncEnabled = true,
            Recursive = true,
            Path = "~/".AsIDocPath(Config),
        };
        return View(new IndexViewModel
        {
            q = q,
            Documents = await Search(q, cancellationToken),
        });
    }

    [ShowInNavBar("Out of Date", showInHorizontal: false), HttpGet("special/out-of-date")]
    public async Task<ActionResult> OutOfDate(CancellationToken cancellationToken)
    {
        var q = new QueryDocumentOptions
        {
            SyncEnabled = true,
            Recursive = true,
            Path = "~/".AsIDocPath(Config),
        };
        var nodes = await Search(q, cancellationToken);
        var changes = nodes.Items.ToDictionary(k => k.Model.Path, n => n.Model.FetchChanges(cancellationToken));
        return View(new IndexViewModel
        {
            q = q,
            Documents = nodes,
        });
    }

    [DocAction(inlineAction: true)]
    [DocActionVisibility(typeof(SpecialFoldersController), nameof(ShouldShowAddFavorite))]
    [DocActionLabel("Add Favorite")]
    [HttpPost("special/favorites/add/{path}")]
    public async Task<ActionResult> AddFavorite(string path, CancellationToken cancellationToken)
    {
        path = Uri.UnescapeDataString(path);
        var nodePath = path.AsIDocPath(Config);
        var node = (IWritableDocument) (await DocumentResolver.UnifiedWritable).CanWrite.GetNodeAtPath(nodePath, cancellationToken);
        (await node.GetWritableMeta(cancellationToken)).Favorited = true;
        await node.WriteMeta(cancellationToken);
        //DocumentResolver.UnifiedWritable.CanWrite.(node as IWritableDocument)
        return View("UnfavoriteButton", path);
    }

    public static async Task<bool> ShouldShowAddFavorite(IReadOnlyNode node, IServiceProvider services)
    {
        //return !ctrlr.GetRequiredService<IUserSpecialFoldersProvider>().Favorites.Contains(node.Path);
        return !(await node.Meta).Favorited; //services.GetRequiredService<IDocumentProviderResolver>().
    }

    public static async Task<bool> ShouldShowRemoveFavorite(IReadOnlyNode node, IServiceProvider services)
    {
        //return ctrlr.GetRequiredService<IUserSpecialFoldersProvider>().Favorites.Contains(node.Path);
        return (await node.Meta).Favorited;
    }

    [DocAction(inlineAction: true)]
    [DocActionVisibility(typeof(SpecialFoldersController), nameof(ShouldShowRemoveFavorite))]
    [DocActionLabel("Remove Favorite")]
    [HttpPost("special/favorites/remove/{path}")]
    public async Task<ActionResult> RemoveFavorite(string path, CancellationToken cancellationToken)
    {
        path = Uri.UnescapeDataString(path);
        var nodePath = path.AsIDocPath(Config);
        var node = (IWritableDocument) (await DocumentResolver.UnifiedWritable).CanWrite.GetNodeAtPath(nodePath, cancellationToken);
        (await node.GetWritableMeta(cancellationToken)).Favorited = false;
        await node.WriteMeta(cancellationToken);
        //UserSpecialFoldersProvider.RemoveFavorite(path.AsIDocPath(Config));
        return View("FavoriteButton", path);
    }

    private PaginatedEnumerableViewModel<IReadOnlyNodeViewModel> SearchInMemory(IEnumerable<IReadOnlyNode> nodes)
    {
        var ret = new PaginatedEnumerableViewModel<IReadOnlyNodeViewModel>
        {

        };
        ret.Items = nodes.Select(n => new IReadOnlyNodeViewModel(n, ret));
        ret.Total = ret.Items.Count();
        return ret;
    }

    [ShowInNavBar("Recently Opened", showInHorizontal: false)]
    [HttpGet("special/recently-opened")]
    public async Task<ActionResult> RecentlyOpened([Bind] QueryDocumentOptions q, CancellationToken cancellationToken)
    {
        // q.OrderByColumn = nameof(IReadOnlyDocumentMeta.LastReadDate);
        return View(new IndexViewModel
        {
            q = q,
            Documents = await Search(q, cancellationToken),
        });
    }

    [ShowInNavBar("Recently Modified", showInHorizontal: false)]
    [HttpGet("special/recently-modified")]
    public async Task<ActionResult> RecentlyModified([Bind] QueryDocumentOptions q, CancellationToken cancellationToken)
    {
        q.OrderByColumn = nameof(IReadOnlyDocumentMeta.LastUpdate);
        return View(new IndexViewModel
        {
            q = q,
            Documents = await Search(q, cancellationToken),
        });
    }

    [ShowInNavBar("Recently Created", showInHorizontal: false)]
    [HttpGet("special/recently-created")]
    public async Task<ActionResult> RecentlyCreated([Bind] QueryDocumentOptions q, CancellationToken cancellationToken)
    {
        q.OrderByColumn = nameof(IReadOnlyDocumentMeta.CreationDate);
        return View(new IndexViewModel
        {
            q = q,
            Documents = await Search(q, cancellationToken),
        });
    }

    protected override void ForceControllerSpecificOptions(QueryDocumentOptions q)
    {
        q.IsInFavorites = true;

        if (q.Path == null || string.IsNullOrEmpty(q.Path.PathValue) || q.Path.PathValue.StartsWith("~/"))
        {
            q.Path = "~/".AsIDocPath(Config);
        }
    }
}