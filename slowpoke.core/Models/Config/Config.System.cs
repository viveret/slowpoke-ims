using slowpoke.core.Models.Config.Attributes;

namespace slowpoke.core.Models.Config;


public partial class Config
{
    public SystemConfig System { get; set; }

    public class SystemConfig
    {
        public SystemConfig()
        {
            Title = string.Empty;
        }

        [Default("theme-slowpoke")]
        public string BodyClass { get; set; }

        [Default("SlowPoke IMS")]
        public string Title { get; set; }

        [Default("fw-bold")]
        public string TitleClass { get; set; }
        
        [Default("/img/slowpoke.png")]
        public string TitleLogoSrc { get; set; }
        
        [Default("transform: rotateY(180deg)")]
        public string TitleLogoStyle { get; set; }

        [Default("95")]
        public string TitleLogoWidth { get; set; }

        [Default("&copy; 2022")]
        public string Footer { get; set; }
    }
}