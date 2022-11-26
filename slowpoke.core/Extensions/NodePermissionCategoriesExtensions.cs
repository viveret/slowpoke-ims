using slowpoke.core.Models.Node;

namespace SlowPokeIMS.Core.Extensions;


public static class NodePermissionCategoriesExtensions
{
    public static NodePermissionCategories<TOut> Transform<T, TOut>(
        this NodePermissionCategories<T> thiz, Func<T, TOut> transformer)
    {
        return new NodePermissionCategories<TOut>
        {
            CanRead = transformer(thiz.CanRead),
            CanWrite = transformer(thiz.CanWrite),
            IsEncrypted = transformer(thiz.IsEncrypted),
            IsRemote = transformer(thiz.IsRemote),
            LimitedToUserOnly = transformer(thiz.LimitedToUserOnly),
            LimitedToMachineOnly = transformer(thiz.LimitedToMachineOnly),
            LimitedToLocalNetworkOnly = transformer(thiz.LimitedToLocalNetworkOnly),
            LimitedToAllowedConnectionsOnly = transformer(thiz.LimitedToAllowedConnectionsOnly),
            UnlimitedUniversalPublicAccess = transformer(thiz.UnlimitedUniversalPublicAccess),
        };
    }

    public static async Task<NodePermissionCategories<TOut>> TransformAsync<T, TOut>(
        this Task<NodePermissionCategories<T>> permsTask, Func<T, Task<TOut>> transformer)
    {
        var perms = await permsTask;
        return new NodePermissionCategories<TOut>
        {
            CanRead = await transformer(perms.CanRead),
            CanWrite = await transformer(perms.CanWrite),
            IsEncrypted = await transformer(perms.IsEncrypted),
            IsRemote = await transformer(perms.IsRemote),
            LimitedToUserOnly = await transformer(perms.LimitedToUserOnly),
            LimitedToMachineOnly = await transformer(perms.LimitedToMachineOnly),
            LimitedToLocalNetworkOnly = await transformer(perms.LimitedToLocalNetworkOnly),
            LimitedToAllowedConnectionsOnly = await transformer(perms.LimitedToAllowedConnectionsOnly),
            UnlimitedUniversalPublicAccess = await transformer(perms.UnlimitedUniversalPublicAccess),
        };
    }
}