using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using slowpoke.core.Services;

namespace SlowPokeIMS.Web.Services;



public class ServingInfoProvider : IServingInfoProvider
{
    public ServingInfoProvider(IServer server)
    {
        var addressFeature = server.Features.Get<IServerAddressesFeature>();

        foreach (var address in addressFeature.Addresses)
        {
            Ip = address.ToString();
            Port = 0;
        }
    }

    public string Ip { get; }

    public short Port { get; }
}