using slowpoke.core.Models.Config.Attributes;
using slowpoke.core.Models.Node.Docs;

namespace slowpoke.core.Models.Config;


public partial class Config
{
    public PathsConfig Paths { get; set; }

    public class PathsConfig
    {
        public PathsConfig(IEnvironment env)
        {
            Env = env;
        }
        
        public const string DocMetaExtension = ".slowpokemeta.json";

        public const string DocMetaExtensionPattern = "*.slowpokemeta.json";

        public string AppRootPath => Env.AppRootPath; // constant
        
        public string ContentRootPath => Env.ContentRootPath; // constant
        
        public string HomePath => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        [Default(".slowpoke")]
        public string PrivateFilesFolderName { get; set; }

        public string PrivateFilesPath => Path.Combine(HomePath, PrivateFilesFolderName);
        
        public IEnvironment Env { get; }
        
        [Default("doc-ids-2-paths")]
        public string DocId2PathFolderName { get; set; }
        
        public string DocId2PathPath => Path.Combine(PrivateFilesPath, DocId2PathFolderName);
        
        [Default("doc-paths-2-ids")]
        public string DocPaths2IdsFolderName { get; set; }

        public string DocPaths2IdsPath => Path.Combine(PrivateFilesPath, DocPaths2IdsFolderName);









        public Dictionary<string, string> ContentTypeToExtensionMap { get; set; } = new Dictionary<string, string>
        {
            { "text", "txt" },
            { "text/*", "txt" },
            { "text/plain", "txt" },
            { "text/markdown", "md" },
            { "text/json", "json" },
            { "text/xml", "xml" },
            { "text/html", "html" },
            { "text/cshtml", "cshtml" },
            { "text/cs", "cs" },
            { "application/pdf", "pdf" },
            { "application/obj", "obj" },
            { "application/csproj", "csproj" },
            { "application/file-list", "filelistabsolute.txt" },
            { "image/png", "png" },
            { "image/bmp", "bmp" },
            { "image/jpg", "jpg" },
            { "image/jpeg", "jpeg" },
            { "image/gif", "gif" },
        };

        public Dictionary<string, FileCategory> ContentTypeToCategoryMap { get; set; } = new Dictionary<string, FileCategory>
        {
            { "text", FileCategory.Regular },
            { "text/*", FileCategory.Regular },
            { "text/plain", FileCategory.Regular },
            { "text/markdown", FileCategory.Regular },
            { "text/json", FileCategory.SourceCodeOrDevelopment },
            { "text/xml", FileCategory.SourceCodeOrDevelopment },
            { "text/html", FileCategory.SourceCodeOrDevelopment },
            { "text/cshtml", FileCategory.SourceCodeOrDevelopment },
            { "text/cs", FileCategory.SourceCodeOrDevelopment },
            { "application/pdf", FileCategory.Regular },
            { "application/obj", FileCategory.SourceCodeOrDevelopment },
            { "application/csproj", FileCategory.SourceCodeOrDevelopment },
            { "application/file-list", FileCategory.SourceCodeOrDevelopment },
            { "image/png", FileCategory.Regular },
            { "image/bmp", FileCategory.Regular },
            { "image/jpg", FileCategory.Regular },
            { "image/jpeg", FileCategory.Regular },
            { "image/gif", FileCategory.Regular },
        };

        public Dictionary<string, FileCategory> ExtensionToCategoryMap { get; set; } = new Dictionary<string, FileCategory>
        {
            { "txt", FileCategory.Regular },
            { "md", FileCategory.Regular },
            { "pdf", FileCategory.Regular },
            { "json", FileCategory.SourceCodeOrDevelopment },
            { "xml", FileCategory.SourceCodeOrDevelopment },
            { "html", FileCategory.SourceCodeOrDevelopment },
            { "cshtml", FileCategory.SourceCodeOrDevelopment },
            { "cs", FileCategory.SourceCodeOrDevelopment },
            { "obj", FileCategory.SourceCodeOrDevelopment },
            { "csproj", FileCategory.SourceCodeOrDevelopment },
            { "filelistabsolute.txt", FileCategory.SourceCodeOrDevelopment },
            { "png", FileCategory.Regular },
            { "bmp", FileCategory.Regular },
            { "jpg", FileCategory.Regular },
            { "jpeg", FileCategory.Regular },
            { "gif", FileCategory.Regular },
        };

        public static string [] osPaths = new string [] {
            "/bin/",
            "/boot/",
            "/cdrom/",
            "/dev/",
            "/lib/",
            "/lib32/",
            "/lib64/",
            "/libx32/",
            "/media/",
            "/mnt/",
            "/opt/",
            "/proc/",
            "/root/",
            "/run/",
            "/sbin/",
            "/snap/",
            "/srv/",
            "/swapfile",
            "/sys/",
            "/tmp/",
        };

        public bool IsOS(string path)
        {
            foreach (var osPath in osPaths)
            {
                if (path.StartsWith(osPath))
                {
                    return true;
                }
            }
            if (path.Contains(DocId2PathFolderName) || path.Contains(DocPaths2IdsFolderName) || path.EndsWith(DocMetaExtension))
            {
                return true;
            }
            return false;
        }

        public Dictionary<string, FileCategory> FolderToCategoryMap { get; set; } = new Dictionary<string, FileCategory>
        {
            { "obj", FileCategory.SourceCodeOrDevelopment },
            { "node_modules", FileCategory.SourceCodeOrDevelopment },
        };
    }
}