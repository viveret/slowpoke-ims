using System.Collections.Generic;
using slowpoke.core.Models.Node;

namespace SlowPokeIMS.Web.ViewModels.System;



public class HostsViewModel
{
    public IEnumerable<ISlowPokeHost> Hosts { get; set; }
    
    public SearchForLocalNetworkHostsResult SearchResults { get; set; }
}