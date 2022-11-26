using slowpoke.core.Models.Configuration;

namespace slowpoke.core.Models.Node.Docs;


public class DocPathUri : DocPathBase
{
    public Uri UriValue
    {
        get
        {
            return new Uri(PathValue);
        }
        // set
        // {
        //     PathValue = value.ToString();
        // }
    }

    public DocPathUri(string pathValue, Config config) : base(pathValue, config)
    {
        if (!Uri.TryCreate(pathValue, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException($"Invalid uri: {pathValue}", nameof(pathValue));
        }
    }

    public override bool IsUri => true;
    
    public override bool IsRelative => false;

    public override bool IsAbsolute => false;

    public override INodePath ConvertToAbsolutePath() => throw new NotImplementedException(); //new DocPathAbsolute(UriValue.LocalPath, Config);

    public override INodePath ConvertToUriPath() => this;
}