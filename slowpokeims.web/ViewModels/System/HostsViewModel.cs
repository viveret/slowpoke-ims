using System.Collections.Generic;
using slowpoke.core.Models.Node;
using slowpoke.core.Services.Scheduled;

namespace SlowPokeIMS.Web.ViewModels.System;



public class HostsViewModel
{
    public IEnumerable<ISlowPokeHost> AllHosts { get; set; }
    
    public SearchForLocalNetworkHostsResult SearchResults { get; set; }
    
    public IScheduledTaskContext SearchForHostsTaskContext { get; set; }
    
    public IEnumerable<ISlowPokeHost> TrustedHosts { get; set; }
    
    public IEnumerable<ISlowPokeHost> KnownButUntrustedHosts { get; set; }
}