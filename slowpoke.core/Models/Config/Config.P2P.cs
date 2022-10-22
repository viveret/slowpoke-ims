using slowpoke.core.Models.Config.Attributes;
using slowpoke.core.Services;

namespace slowpoke.core.Models.Config;


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

        public string ServingAddress => servingInfoProvider.Ip;
        
        [Default(false)]
        public bool AutoCacheLocalNetworkHosts { get; set; }
        
        [Default(false)]
        public bool EnableCacheLocalNetworkHosts { get; set; }
        
        [Default(false)]
        public bool AllowSearchForLocalNetworkHosts { get; set; }
        
        [Default(true)]
        public bool SyncEnabled { get; set; } = true;


        public string[] KnownHosts { get; set; } = new string[]{ "https://127.0.0.1:5001" };// Array.Empty<string>();
    }
}