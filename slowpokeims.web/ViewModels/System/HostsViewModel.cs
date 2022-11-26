using System.Collections.Generic;
using slowpoke.core.Client;
using slowpoke.core.Models.Node;
using slowpoke.core.Services.Scheduled;

namespace SlowPokeIMS.Web.ViewModels.System;



public class HostsViewModel
{
    public IEnumerable<SlowPokeHostViewModel> AllHosts { get; set; }
    
    public IEnumerable<SlowPokeHostViewModel> TrustedHosts { get; set; }
    
    public IEnumerable<SlowPokeHostViewModel> KnownButUntrustedHosts { get; set; }
    
    public SearchForLocalNetworkHostsResult SearchResults { get; set; }
    
    public IScheduledTaskContext SearchForHostsTaskContext { get; set; }
    
    public SlowPokeHostViewModel Host { get; set; }
}