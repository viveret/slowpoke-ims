using System.Net;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using slowpoke.core.Models.Configuration.Attributes;
using slowpoke.core.Services;

namespace slowpoke.core.Models.Configuration;


public partial class Config
{
    public P2PConfig P2P { get; set; }

    public class P2PConfig
    {
        private readonly IServingInfoProvider servingInfoProvider;

        public P2PConfig(IServingInfoProvider servingInfoProvider)
        {
            this.servingInfoProvider = servingInfoProvider;
        }

        // Empty constructor for deserialization
        public P2PConfig() { }

        [JsonIgnore, IgnoreDataMember]
        public string ServingAddress => servingInfoProvider.Ip;
        
        [JsonIgnore, IgnoreDataMember]
        public string? LocalAddress
        {
            get
            {
                var localAddresses = Dns.GetHostAddresses(Dns.GetHostName());
                var localIp = localAddresses.Where(ip => !ip.ToString().StartsWith("127.0.") && !ip.ToString().Equals("::1")).FirstOrDefault();
                return localIp?.ToString();
            }
        }
        
        [Default(false)]
        public bool AutoCacheLocalNetworkHosts { get; set; }
        
        [Default(false)]
        public bool EnableCacheLocalNetworkHosts { get; set; }
        
        [Default(true)]
        public bool AllowSearchForLocalNetworkHosts { get; set; }
        
        [Default(true)]
        public bool SyncEnabled { get; set; } = true;

        public List<string> TrustedHosts { get; set; } = new ();

        public List<string> KnownButUntrustedHosts { get; set; } = new ();
    }
}