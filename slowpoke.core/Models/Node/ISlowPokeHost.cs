namespace slowpoke.core.Models.Node;


public interface ISlowPokeHost: ISlowPokeId
{
    slowpoke.core.Services.Node.Docs.IDocumentProviderResolver DocProviderResolver { get; }
}