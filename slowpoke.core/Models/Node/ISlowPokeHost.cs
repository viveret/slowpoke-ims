namespace slowpoke.core.Models.Node;


public interface ISlowPokeHost: ISlowPokeId
{
    Task<Configuration.Config> Config { get; }

    slowpoke.core.Services.Node.Docs.IDocumentProviderResolver DocProviderResolver { get; }
}