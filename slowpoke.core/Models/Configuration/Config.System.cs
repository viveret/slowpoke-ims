using System.Reflection;
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

        public string SoftwareVersion => Assembly.GetExecutingAssembly().ImageRuntimeVersion;
        
        public DateTime SoftwareVersionDate
        {
            get
            {
                var path = Assembly.GetExecutingAssembly().Location!;
                var creationTime = File.GetCreationTimeUtc(path);
                var lastWriteTime = File.GetLastWriteTimeUtc(path);
                return new DateTime(Math.Max(creationTime.Ticks, lastWriteTime.Ticks));
            }
        }
        
        public Version EnvVersion => Environment.Version;
        
        public string UserRunningAs => Environment.UserName;
        
        public string MachineRunningAs => Environment.MachineName;
    }
}