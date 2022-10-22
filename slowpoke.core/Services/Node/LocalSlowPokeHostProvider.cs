using slowpoke.core.Models;
using slowpoke.core.Models.Node;
using slowpoke.core.Services.Node.Docs;

namespace slowpoke.core.Services.Node;


public class LocalSlowPokeHostProvider: ISlowPokeHostProvider
{
    public LocalSlowPokeHostProvider(IDocumentProviderResolver current)
    {
    }

    public IEnumerable<ISlowPokeHost> All { get; }
    
    public IEnumerable<ISlowPokeHost> AllExceptCurrent { get; }
    
    public ISlowPokeHost Current { get; }
}