using slowpoke.core.Services.Node.Docs;

namespace slowpoke.core.Services.Node.Docs;



public interface IReadOnlyRemoteDocumentResolver: IReadOnlyDocumentResolver
{
    Uri Endpoint { get; }

    bool IsLocalHost { get; }
}