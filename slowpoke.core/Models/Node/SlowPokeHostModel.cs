using slowpoke.core.Services.Node.Docs;

namespace slowpoke.core.Models.Node;


public class SlowPokeHostModel : SlowPokeIdModel, ISlowPokeHost
{
    public IDocumentProviderResolver DocProviderResolver { get; set; }
}