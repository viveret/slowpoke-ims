using System.Text;

namespace slowpoke.core.Models.Node.Docs;


public static class DocPathExtensions
{
    public static INodePath AsIDocPath(this string path, Config.Config config)
    {
        var fullDocsPath = System.Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (path.StartsWith("/"))
        {
            return new DocPathAbsolute(path, config);
        }
        else if (path.StartsWith("https://") || path.StartsWith("http://"))
        {
            return new DocPathUri(path, config);
        }
        else if (path.StartsWith("~/") || path.StartsWith(fullDocsPath))
        {
            return new DocPathRelative(path, config);
        }
        else
        {
            return new DocPathRelative(path, config);
        }
    }

    public static bool HasValue(this INodePath path) => !string.IsNullOrWhiteSpace(path?.PathValue);

    public static bool HasValue(this string str) => !string.IsNullOrWhiteSpace(str);

    public static string GetFullExtension(this string str)
    {
        var extBuilder = new StringBuilder();
        var lastDotPosition = str.Length - 1;
        var dotPosition = str.Length - 1;
        while (lastDotPosition > 1 && (dotPosition = str.LastIndexOf('.', lastDotPosition - 1)) > 0)
        {
            var ext = str.Substring(dotPosition, lastDotPosition - dotPosition + 1);
            extBuilder.Insert(0, ext);
            lastDotPosition = dotPosition;
        }
        
        return extBuilder.ToString().TrimStart('.');
    }
}