using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using slowpoke.core.Services;

namespace SlowPokeIMS.Core.Services;



public class ServingInfoProvider : IServingInfoProvider
{
    private readonly IServer server;

    public ServingInfoProvider(IServer server)
    {
        this.server = server;
    }

    public string Ip
    {
        get
        {
            var addressFeature = server.Features.Get<IServerAddressesFeature>();

            if (addressFeature == null)
            {
                throw new ArgumentException("Server is not bound to any IP");
            }
            
            foreach (var address in addressFeature.Addresses)
            {
                return address.ToString();
            }

            throw new ArgumentException("Server is not bound to any IP");
        }
    }

    public short Port { get; }
}