using slowpoke.core.Models.Configuration.Attributes;

namespace slowpoke.core.Models.Configuration;


public partial class Config
{
    public ContentPreviewConfig ContentPreview { get; set; }

    public class ContentPreviewConfig
    {
        // Empty constructor for deserialization
        public ContentPreviewConfig()
        {
        }

        [Default(5)]
        public int NumLines { get; set; } = 5;
    }
}