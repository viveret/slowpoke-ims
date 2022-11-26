using slowpoke.core.Models.Configuration;

namespace slowpoke.core.Models.Node.Docs;


public class DocPathAbsolute : DocPathBase
{
    public DocPathAbsolute(string pathValue, Config config) : base(pathValue, config)
    {
    }

    public override bool IsUri => false;

    public override bool IsRelative => false;
    
    public override bool IsAbsolute => true;

    public override INodePath ConvertToAbsolutePath() => this;
    
    public override INodePath ConvertToUriPath() => throw new NotImplementedException();
}