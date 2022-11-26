using slowpoke.core.Client;
using slowpoke.core.Services.Node;
using slowpoke.core.Services.Node.Docs;

namespace slowpoke.core.Models.Node;


public class SlowPokeHost : ISlowPokeHost
{
    private ISlowPokeClient slowPokeClient;
    private LocalSlowPokeHostProvider localSlowPokeHostProvider;
    private SlowPokeHostModel? cachedInfo;

    protected SlowPokeHost(ISlowPokeClient slowPokeClient, LocalSlowPokeHostProvider localSlowPokeHostProvider)
    {
        this.slowPokeClient = slowPokeClient;
        this.localSlowPokeHostProvider = localSlowPokeHostProvider;
    }

    public static async Task<SlowPokeHost> Resolve(ISlowPokeClient slowPokeClient, LocalSlowPokeHostProvider localSlowPokeHostProvider)
    {
        var host = new SlowPokeHost(slowPokeClient, localSlowPokeHostProvider);
        await host.GetCachableFields();
        return host;
    }

    public IDocumentProviderResolver DocProviderResolver => throw new NotImplementedException();

    private string ToInfoPropertyName(string v)
    {
        var p = Char.ToLower(v[0]);
        return "hostInfo { " + p + v.Substring(1) + " }";
    }

    public string Label => cachedInfo!.Label;

    private async Task GetCachableFields()
    {
        // todo: this needs to be done async somehow, maybe pass in constructor so we know for sure this data is resolvable
        cachedInfo = new SlowPokeHostModel
        {
            Label = (await slowPokeClient.GraphQLQuery<QueryHostModelResult>(ToInfoPropertyName(nameof(Label)), CancellationToken.None)).hostInfo?.Label ?? string.Empty,
        };
    }

    public Guid Guid => cachedInfo!.Guid;

    public Guid[] GuidAlternatives => cachedInfo!.GuidAlternatives; // response => response.Split(',').Select(s => Guid.Parse(s)).ToArray()

    public Uri Endpoint => cachedInfo!.Endpoint;

    public Uri[] EndpointAlternatives => cachedInfo!.EndpointAlternatives; // response => response.Split(',').Select(s => new Uri(s)).ToArray()

    public string RawIdType => GetType().FullName!;

    public string RawId => slowPokeClient.Endpoint.ToString();



    private class QueryHostModelResult
    {
        public SlowPokeHostModel? hostInfo { get; set; }
    }
}