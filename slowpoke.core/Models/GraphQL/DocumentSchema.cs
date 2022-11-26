using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using slowpoke.core.Models.Node;
using slowpoke.core.Models.Node.Docs;
using slowpoke.core.Services.Node.Docs;

namespace slowpoke.core.Models.GraphQL;


public class DocumentSchema : Schema
{
    public class DocumentQuery : ObjectGraphType<IReadOnlyDocument>
    {
        public DocumentQuery(IDocumentProviderResolver documentResolver)
        {
            Name = "Document";

            Field<BooleanGraphType>("CanRead")
                .ResolveAsync(async ctx => (await documentResolver.ResolveReadable).CanRead);
        }
    }

    public DocumentSchema(IServiceProvider services) : base(services)
    {
        Query = services.GetRequiredService<DocumentQuery>();
    }
}