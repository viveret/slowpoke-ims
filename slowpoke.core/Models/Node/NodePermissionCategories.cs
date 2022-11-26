using SlowPokeIMS.Core.Collections;

namespace slowpoke.core.Models.Node;


public class NodePermissionCategories<T>
{
    public T CanRead { get; set; }
    
    public T CanWrite { get; set; }
    
    public T LimitedToUserOnly { get; set; }
    
    public T LimitedToMachineOnly { get; set; }
    
    public T LimitedToLocalNetworkOnly { get; set; }
    
    public T LimitedToAllowedConnectionsOnly { get; set; }
    
    public T UnlimitedUniversalPublicAccess { get; set; }
    
    public T IsEncrypted { get; set; }

    public T IsRemote { get; set; }


    public static async Task<NodePermissionCategories<IEnumerable<T>>> FilterPermissions(
        Task<IEnumerable<T>> list, Func<T, Task<bool>> valueConditional)
    {
        var items = await list;
        return new NodePermissionCategories<IEnumerable<T>>
        {
            CanRead = await items.WhereAsync(valueConditional),
            CanWrite = await items.WhereAsync(valueConditional),
            IsEncrypted = await items.WhereAsync(valueConditional),
            LimitedToUserOnly = await items.WhereAsync(valueConditional),
            LimitedToMachineOnly = await items.WhereAsync(valueConditional),
            LimitedToLocalNetworkOnly = await items.WhereAsync(valueConditional),
            LimitedToAllowedConnectionsOnly = await items.WhereAsync(valueConditional),
            UnlimitedUniversalPublicAccess = await items.WhereAsync(valueConditional),
            IsRemote = Enumerable.Empty<T>(),
        };
    }
}