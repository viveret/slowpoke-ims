using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;


namespace slowpoke.core.Models.GraphQL;

public class ApiSchema : Schema
{
    public class ApiQuery : ObjectGraphType
    {
        public ApiQuery()
        {
            Name = "Api";

            Field<HostInfoSchema.HostInfoQuery>("HostInfo")
                .Resolve(ctx => new Object());

            Field<DocumentSchema.DocumentQuery>("Docs")
                .Resolve(ctx => new Object());
        }
    }

    public ApiSchema(IServiceProvider services) : base(services)
    {
        Query = services.GetRequiredService<ApiQuery>();
    }
}