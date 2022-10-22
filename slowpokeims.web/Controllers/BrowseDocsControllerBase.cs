using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using slowpoke.core.Models;
using slowpoke.core.Models.Config;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services;
using slowpoke.core.Services.Node;
using slowpoke.core.Services.Node.Docs;
using SlowPokeIMS.Web.Extensions;
using SlowPokeIMS.Web.ViewModels;
using SlowPokeIMS.Web.ViewModels.Home;

namespace SlowPokeIMS.Web.Controllers;


public abstract class BrowseDocsControllerBase : SlowPokeControllerBase
{
    public IUserSpecialFoldersProvider UserSpecialFoldersProvider { get; }

    public BrowseDocsControllerBase(
        IDocumentProviderResolver documentResolver,
        IUserSpecialFoldersProvider userSpecialFoldersProvider,
        Config config): base(documentResolver, config) {
            UserSpecialFoldersProvider = userSpecialFoldersProvider;
        }
    
    protected virtual PaginatedEnumerableViewModel<IReadOnlyNodeViewModel> Search(QueryDocumentOptions q, CancellationToken cancellationToken)
    {
        q.PageSize = GetPageSizeForViewType(q.ItemView);
        var total = DocumentResolver.UnifiedReadable.CanRead.GetCountOfNodes(q, cancellationToken);
        var model = new PaginatedEnumerableViewModel<IReadOnlyNodeViewModel>
        {
            Total = total,
            PageToUrl = (pg) => Url.Action(nameof(Index), q.CopyButChangePage(pg)),
            ItemViewTypeToUrl = (vt) => Url.Action(nameof(Index), q.CopyButChangeViewType(vt)),
            Offset = q.Offset,
            PageSize = q.PageSize,
            TitleUrlResolver = (m) => m.Model.Path.IsDocument ? Url.Action("Details", "Home", new { path = m.Model.Path.PathValue }) : Url.Action("Index", "Home", q.CopyButChangeFolder(m.Model.Path)),
        };

        model.Items = GetItemsForViewModel(q, model, cancellationToken);

        return model;
    }

    protected virtual IEnumerable<IReadOnlyNodeViewModel> GetItemsForViewModel(QueryDocumentOptions q, IPaginatedEnumerableViewModel parent, CancellationToken cancellationToken)
    {
        var ur = DocumentResolver.UnifiedReadable;
        var r = ur.CanRead;
        var nodes = r.GetNodes(q, cancellationToken) ?? throw new Exception("Missing nodes");
        return nodes.Select(n => new IReadOnlyNodeViewModel(n, parent)); // nodes == null ? Enumerable.Empty<IReadOnlyNodeViewModel>() : 
    }

    protected int GetPageSizeForViewType(ItemViewType? itemView)
    {
        return (itemView ?? ItemViewType.Default) switch
        {
            ItemViewType.Default => 10,
            ItemViewType.SmallItemList => 20,
            ItemViewType.LargeItemList => 10,
            ItemViewType.SmallItemGrid => 20,
            ItemViewType.LargeItemGrid => 10,
            ItemViewType.SmallCards => 20,
            ItemViewType.LargeCards => 10,
            _ => throw new NotSupportedException($"Item view type {itemView} not supported")
        };
    }
}