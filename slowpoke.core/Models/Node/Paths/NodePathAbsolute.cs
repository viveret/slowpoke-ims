namespace slowpoke.core.Models.Node.Docs;


public class DocPathAbsolute : DocPathBase
{
    public DocPathAbsolute(string pathValue, Config.Config config) : base(pathValue, config)
    {
    }

    public override bool IsUri => false;

    public override bool IsRelative => true;
    
    public override bool IsAbsolute => true;

    public override INodePath ConvertToAbsolutePath() => this;
    
    public override INodePath ConvertToUriPath() => throw new NotImplementedException();
}