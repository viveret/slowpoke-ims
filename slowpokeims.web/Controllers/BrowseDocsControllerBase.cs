using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using slowpoke.core.Models;
using slowpoke.core.Models.Configuration;
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
    //public IUserSpecialFoldersProvider UserSpecialFoldersProvider { get; }

    public BrowseDocsControllerBase(
        IDocumentProviderResolver documentResolver,
        //IUserSpecialFoldersProvider userSpecialFoldersProvider,
        Config config): base(documentResolver, config) {
            // UserSpecialFoldersProvider = userSpecialFoldersProvider;
        }
    
    protected virtual async Task<PaginatedEnumerableViewModel<IReadOnlyNodeViewModel>> Search(QueryDocumentOptions q, CancellationToken cancellationToken)
    {
        q.PageSize = GetPageSizeForViewType(q.ItemView);
        var total = await (await DocumentResolver.UnifiedReadable).CanRead.GetCountOfNodes(q, cancellationToken);
        var model = new PaginatedEnumerableViewModel<IReadOnlyNodeViewModel>
        {
            Total = total,
            PageToUrl = (pg) => Url.Action(nameof(Index), q.CopyButChangePage(pg)),
            ItemViewTypeToUrl = (vt) => Url.Action(nameof(Index), q.CopyButChangeViewType(vt)),
            Offset = q.Offset,
            PageSize = q.PageSize,
            TitleUrlResolver = (m) => DefaultTitleUrlResolver(m, q),
        };

        model.Items = await GetItemsForViewModel(q, model, cancellationToken);

        return model;
    }

    protected virtual string DefaultControllerNameForTitleUrlResolver(IReadOnlyNodeViewModel m, QueryDocumentOptions q)
    {
        return ControllerContext.ActionDescriptor.ControllerName;
    }

    protected virtual string DefaultDocActionNameForTitleUrlResolver(IReadOnlyNodeViewModel m, QueryDocumentOptions q)
    {
        return "Details";
    }

    protected virtual string DefaultFolderActionNameForTitleUrlResolver(IReadOnlyNodeViewModel m, QueryDocumentOptions q)
    {
        return "Index";
    }

    protected virtual string DefaultTitleUrlResolver(IReadOnlyNodeViewModel m, QueryDocumentOptions q)
    {
        var controller = DefaultControllerNameForTitleUrlResolver(m, q);
        var actionNameForDoc = DefaultDocActionNameForTitleUrlResolver(m, q);
        var actionNameForFolder = DefaultFolderActionNameForTitleUrlResolver(m, q);
        return m.Model.Path.IsDocument ? Url.Action(actionNameForDoc, controller, new { path = m.Model.Path.PathValue }) : Url.Action(actionNameForFolder, controller, q.CopyButChangeFolder(m.Model.Path));
    }

    protected virtual async Task<IEnumerable<IReadOnlyNodeViewModel>> GetItemsForViewModel(QueryDocumentOptions q, IPaginatedEnumerableViewModel parent, CancellationToken cancellationToken)
    {
        var ur = await DocumentResolver.UnifiedReadable;
        var r = ur.CanRead;
        var nodes = (await r.GetNodes(q, cancellationToken)) ?? throw new Exception("Missing nodes");
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