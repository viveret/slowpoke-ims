using slowpoke.core.Client;
using slowpoke.core.Models.Node;

namespace SlowPokeIMS.Web.ViewModels.System;



public class SlowPokeHostViewModel
{
    public ISlowPokeHost Host { get; }

    public ISlowPokeClient Client { get; }

    public SlowPokeHostViewModel(ISlowPokeHost host, ISlowPokeClient client)
    {
        Host = host;
        Client = client;
    }
}