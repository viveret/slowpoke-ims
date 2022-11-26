using slowpoke.core.Models.Configuration.Attributes;

namespace slowpoke.core.Models.Configuration;


public partial class Config
{
    public SystemConfig System { get; set; }

    public class SystemConfig
    {
        // Empty constructor for deserialization
        public SystemConfig()
        {
        }

        [Default("SlowPoke IMS")]
        public string Title { get; set; } = string.Empty;
    }
}