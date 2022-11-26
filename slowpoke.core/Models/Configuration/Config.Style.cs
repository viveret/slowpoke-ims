using slowpoke.core.Models.Configuration.Attributes;

namespace slowpoke.core.Models.Configuration;


public partial class Config
{
    public StyleConfig Style { get; set; }

    public class StyleConfig
    {
        // Empty constructor for deserialization
        public StyleConfig()
        {
        }

        [Default("theme-slowpoke")]
        public string BodyClass { get; set; } = string.Empty;

        [Default("fw-bold")]
        public string TitleClass { get; set; } = string.Empty;
        
        [Default("/img/slowpoke.png")]
        public string TitleLogoSrc { get; set; } = string.Empty;
        
        [Default("transform: rotateY(180deg)")]
        public string TitleLogoStyle { get; set; } = string.Empty;

        [Default("95")]
        public string TitleLogoWidth { get; set; } = string.Empty;

        [Default("&copy; 2022")]
        public string Footer { get; set; } = string.Empty;

        [Default("&#x1F4C1;")]
        public string FolderIcon { get; set; } = string.Empty;
    }
}