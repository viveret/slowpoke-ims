using slowpoke.core.Models.Node;

namespace slowpoke.core.Services.Node;


public interface ISlowPokeHostProvider
{
    IEnumerable<ISlowPokeHost> All { get; }
    
    IEnumerable<ISlowPokeHost> AllExceptCurrent { get; }
    
    ISlowPokeHost Current { get; }

    SearchForLocalNetworkHostsResult SearchForLocalNetworkHosts(CancellationToken cancellationToken);
}