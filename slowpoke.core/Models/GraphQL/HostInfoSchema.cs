using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using slowpoke.core.Models.Node;
using slowpoke.core.Services.Node;


namespace slowpoke.core.Models.GraphQL;

public class HostInfoSchema : Schema
{
    public class HostInfoQuery : ObjectGraphType<object>
    {
        public HostInfoQuery(ISlowPokeHostProvider slowPokeHostProvider)
        {
            Name = "HostInfo";

            Field<StringGraphType>(nameof(ISlowPokeHost.Label))
                .Resolve(ctx => slowPokeHostProvider.Current.Label);

            Field<GuidGraphType>(nameof(ISlowPokeHost.Guid))
                .Resolve(ctx => slowPokeHostProvider.Current.Guid);

            Field<ListGraphType<GuidGraphType>>(nameof(ISlowPokeHost.GuidAlternatives))
                .Resolve(ctx => slowPokeHostProvider.Current.GuidAlternatives);

            Field<UriGraphType>(nameof(ISlowPokeHost.Endpoint))
                .Resolve(ctx => slowPokeHostProvider.Current.Endpoint);

            Field<ListGraphType<UriGraphType>>(nameof(ISlowPokeHost.EndpointAlternatives))
                .Resolve(ctx => slowPokeHostProvider.Current.EndpointAlternatives);

            Field<StringGraphType>(nameof(ISlowPokeHost.RawIdType))
                .Resolve(ctx => slowPokeHostProvider.Current.RawIdType);

            Field<StringGraphType>(nameof(ISlowPokeHost.RawId))
                .Resolve(ctx => slowPokeHostProvider.Current.RawId);
        }
    }

    public HostInfoSchema(IServiceProvider services) : base(services)
    {
        Query = services.GetRequiredService<HostInfoQuery>();
    }
}