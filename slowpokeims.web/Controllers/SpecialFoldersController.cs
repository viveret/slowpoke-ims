using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using slowpoke.core.Models.Config;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services;
using slowpoke.core.Services.Node;
using slowpoke.core.Services.Node.Docs;
using SlowPokeIMS.Web.Controllers.Attributes;
using SlowPokeIMS.Web.ViewModels;
using SlowPokeIMS.Web.ViewModels.SpecialFolders;

namespace SlowPokeIMS.Web.Controllers;


[ShowInNavBar("Special Folders", showInHorizontal: false)]
[DocController]
public class SpecialFoldersController : BrowseDocsControllerBase
{
    public SpecialFoldersController(
        IDocumentProviderResolver documentResolver,
        IUserSpecialFoldersProvider userSpecialFoldersProvider,
        Config config): base(documentResolver, userSpecialFoldersProvider, config)
    {
    }

    [ShowInNavBar("Archived", showInHorizontal: false), HttpGet("special/archived")]
    public ActionResult Archived([FromQuery] QueryDocumentOptions q, CancellationToken cancellationToken)
    {
        q.Path ??= Config.Paths.HomePath.AsIDocPath(Config);
        q.Recursive = true;
        q.PageSize = GetPageSizeForViewType(q.ItemView);
        return View(new IndexViewModel
        {
            q = q,
            Documents = Search(q, cancellationToken),
        });
    }

    [ShowInNavBar("Favorites", showInHorizontal: false), HttpGet("special/favorites")]
    public ActionResult Favorites(CancellationToken cancellationToken)
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
            Documents = Search(q, cancellationToken),
        });
    }

    [ShowInNavBar("Sync Enabled", showInHorizontal: false), HttpGet("special/sync-enabled")]
    public ActionResult SyncEnabled(CancellationToken cancellationToken)
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
            Documents = Search(q, cancellationToken),
        });
    }

    [ShowInNavBar("Out of Date", showInHorizontal: false), HttpGet("special/out-of-date")]
    public ActionResult OutOfDate(CancellationToken cancellationToken)
    {
        var q = new QueryDocumentOptions
        {
            SyncEnabled = true,
            Recursive = true,
            Path = "~/".AsIDocPath(Config),
        };
        var nodes = Search(q, cancellationToken);
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
    public ActionResult AddFavorite(string path, CancellationToken cancellationToken)
    {
        path = Uri.UnescapeDataString(path);
        var nodePath = path.AsIDocPath(Config);
        var node = (IWritableDocument) DocumentResolver.UnifiedWritable.CanWrite.GetNodeAtPath(nodePath, cancellationToken);
        node.GetWritableMeta(cancellationToken).Favorited = true;
        node.WriteMeta(cancellationToken);
        //DocumentResolver.UnifiedWritable.CanWrite.(node as IWritableDocument)
        return View("UnfavoriteButton", path);
    }

    public static bool ShouldShowAddFavorite(IReadOnlyNode node, IServiceProvider services)
    {
        //return !ctrlr.GetRequiredService<IUserSpecialFoldersProvider>().Favorites.Contains(node.Path);
        return !node.Meta.Favorited; //services.GetRequiredService<IDocumentProviderResolver>().
    }

    public static bool ShouldShowRemoveFavorite(IReadOnlyNode node, IServiceProvider services)
    {
        //return ctrlr.GetRequiredService<IUserSpecialFoldersProvider>().Favorites.Contains(node.Path);
        return node.Meta.Favorited;
    }

    [DocAction(inlineAction: true)]
    [DocActionVisibility(typeof(SpecialFoldersController), nameof(ShouldShowRemoveFavorite))]
    [DocActionLabel("Remove Favorite")]
    [HttpPost("special/favorites/remove/{path}")]
    public ActionResult RemoveFavorite(string path, CancellationToken cancellationToken)
    {
        path = Uri.UnescapeDataString(path);
        var nodePath = path.AsIDocPath(Config);
        var node = (IWritableDocument) DocumentResolver.UnifiedWritable.CanWrite.GetNodeAtPath(nodePath, cancellationToken);
        node.GetWritableMeta(cancellationToken).Favorited = false;
        node.WriteMeta(cancellationToken);
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
    public ActionResult RecentlyOpened([Bind] QueryDocumentOptions q, CancellationToken cancellationToken)
    {
        // q.OrderByColumn = nameof(IReadOnlyDocumentMeta.LastReadDate);
        return View(new IndexViewModel
        {
            q = q,
            Documents = Search(q, cancellationToken),
        });
    }

    [ShowInNavBar("Recently Modified", showInHorizontal: false)]
    [HttpGet("special/recently-modified")]
    public ActionResult RecentlyModified([Bind] QueryDocumentOptions q, CancellationToken cancellationToken)
    {
        q.OrderByColumn = nameof(IReadOnlyDocumentMeta.LastUpdate);
        return View(new IndexViewModel
        {
            q = q,
            Documents = Search(q, cancellationToken),
        });
    }

    [ShowInNavBar("Recently Created", showInHorizontal: false)]
    [HttpGet("special/recently-created")]
    public ActionResult RecentlyCreated([Bind] QueryDocumentOptions q, CancellationToken cancellationToken)
    {
        q.OrderByColumn = nameof(IReadOnlyDocumentMeta.CreationDate);
        return View(new IndexViewModel
        {
            q = q,
            Documents = Search(q, cancellationToken),
        });
    }
}