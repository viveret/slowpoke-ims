using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Services.Node.Docs;

namespace slowpoke.core.Models.Node;


public class SlowPokeHostModel : SlowPokeIdModel, ISlowPokeHost
{
    public IDocumentProviderResolver? DocProviderResolver { get; set; }

    [JsonIgnore, IgnoreDataMember]
    public Task<Config> Config => Task.FromResult(ConfigSynch);
    
    public Config ConfigSynch { get; set; } = new Config();
}