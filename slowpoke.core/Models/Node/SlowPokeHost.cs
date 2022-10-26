using slowpoke.core.Client;
using slowpoke.core.Services.Node;
using slowpoke.core.Services.Node.Docs;

namespace slowpoke.core.Models.Node;


public class SlowPokeHost : ISlowPokeHost
{
    private ISlowPokeClient slowPokeClient;
    private LocalSlowPokeHostProvider localSlowPokeHostProvider;

    public SlowPokeHost(ISlowPokeClient slowPokeClient, LocalSlowPokeHostProvider localSlowPokeHostProvider)
    {
        this.slowPokeClient = slowPokeClient;
        this.localSlowPokeHostProvider = localSlowPokeHostProvider;
    }

    public IDocumentProviderResolver DocProviderResolver => throw new NotImplementedException();

    private string ToInfoPropertyName(string v)
    {
        var p = Char.ToLower(v[0]);
        return "hostInfo { " + p + v.Substring(1) + " }";
    }

    public string Label => slowPokeClient.GraphQLQuery<QueryHostModelResult>(ToInfoPropertyName(nameof(Label)), CancellationToken.None).hostInfo.Label;

    public Guid Guid => slowPokeClient.GraphQLQuery<QueryHostModelResult>(ToInfoPropertyName(nameof(Guid)), CancellationToken.None).hostInfo.Guid;

    public Guid[] GuidAlternatives => slowPokeClient.GraphQLQuery<QueryHostModelResult>(ToInfoPropertyName(nameof(GuidAlternatives)), CancellationToken.None).hostInfo.GuidAlternatives; // response => response.Split(',').Select(s => Guid.Parse(s)).ToArray()

    public Uri Endpoint => slowPokeClient.GraphQLQuery<QueryHostModelResult>(ToInfoPropertyName(nameof(Endpoint)), CancellationToken.None).hostInfo.Endpoint;

    public Uri[] EndpointAlternatives => slowPokeClient.GraphQLQuery<QueryHostModelResult>(ToInfoPropertyName(nameof(EndpointAlternatives)), CancellationToken.None).hostInfo.EndpointAlternatives; // response => response.Split(',').Select(s => new Uri(s)).ToArray()

    public string RawIdType => GetType().FullName;

    public string RawId => slowPokeClient.Endpoint.ToString();



    private class QueryHostModelResult
    {
        public SlowPokeHostModel hostInfo { get; set; }
    }
}