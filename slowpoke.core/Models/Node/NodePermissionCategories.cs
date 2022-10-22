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

    public NodePermissionCategories<TOut> Transform<TOut>(Func<T, TOut> transformer)
    {
        return new NodePermissionCategories<TOut>
        {
            CanRead = transformer(this.CanRead),
            CanWrite = transformer(this.CanWrite),
            IsEncrypted = transformer(this.IsEncrypted),
            IsRemote = transformer(this.IsRemote),
            LimitedToUserOnly = transformer(this.LimitedToUserOnly),
            LimitedToMachineOnly = transformer(this.LimitedToMachineOnly),
            LimitedToLocalNetworkOnly = transformer(this.LimitedToLocalNetworkOnly),
            LimitedToAllowedConnectionsOnly = transformer(this.LimitedToAllowedConnectionsOnly),
            UnlimitedUniversalPublicAccess = transformer(this.UnlimitedUniversalPublicAccess),
        };
    }
}