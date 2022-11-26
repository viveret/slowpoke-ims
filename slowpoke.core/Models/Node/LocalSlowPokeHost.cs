using slowpoke.core.Models.Configuration;
using slowpoke.core.Services.Node.Docs;

namespace slowpoke.core.Models.Node;

public class LocalSlowPokeHost : ISlowPokeHost
{
    public LocalSlowPokeHost(
        Config config,
        IDocumentProviderResolver docProviderResolver)
    {
        this.config = config;
        DocProviderResolver = docProviderResolver;
        if (docProviderResolver is LocalDocumentProviderResolver local)
        {
            local.AssignLocalHost(this);
        }
    }

    private readonly Config config;

    public IDocumentProviderResolver DocProviderResolver { get; }

    public string Label => "localhost";

    public Guid Guid => throw new NotImplementedException();

    public Guid[] GuidAlternatives => Array.Empty<Guid>();

    public Uri Endpoint => new Uri("localhost");

    public Uri[] EndpointAlternatives => Array.Empty<Uri>();

    public string RawIdType => string.Empty;

    public string RawId => string.Empty;

    public Task<Config> Config => Task.FromResult(config);
}