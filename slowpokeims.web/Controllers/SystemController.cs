using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using slowpoke.core.Client;
using slowpoke.core.Models.Configuration;
using slowpoke.core.Models.Identity;
using slowpoke.core.Models.Node;
using slowpoke.core.Services.Identity;
using slowpoke.core.Services.Node;
using slowpoke.core.Services.Node.Docs;
using slowpoke.core.Services.Scheduled;
using slowpoke.core.Services.Scheduled.Tasks;
using SlowPokeIMS.Web.Controllers.Attributes;
using SlowPokeIMS.Web.ViewModels.System;

namespace SlowPokeIMS.Web.Controllers;


[ShowInNavBar]
public class SystemController : Controller
{
    private readonly Config config;
    private readonly IDocumentProviderResolver documentProviderResolver;
    private readonly IScheduledTaskManager scheduledTaskManager;
    private readonly IIdentityAuthenticationService IdentityAuthenticationService;
    private readonly ISlowPokeHostProvider slowPokeHostProvider;

    public SystemController(
        Config config,
        IDocumentProviderResolver documentProviderResolver,
        IScheduledTaskManager scheduledTaskManager,
        IIdentityAuthenticationService identityAuthenticationService,
        ISlowPokeHostProvider slowPokeHostProvider)
    {
        this.config = config;
        this.documentProviderResolver = documentProviderResolver;
        this.scheduledTaskManager = scheduledTaskManager;
        IdentityAuthenticationService = identityAuthenticationService;
        this.slowPokeHostProvider = slowPokeHostProvider;
    }

    [HttpGet("system/info"), ShowInNavBar("System Info")]    
    public ActionResult Index()
    {
        var process = Process.GetCurrentProcess();
        return View(new IndexViewModel
        {
            MachineName = process.MachineName,
            ProcessName = process.ProcessName,
            StartTime = process.StartTime,
            ThreadCount = process.Threads.Count,

            PagedMemorySize64 = process.PagedMemorySize64,
            PagedSystemMemorySize64 = process.PagedSystemMemorySize64,
            NonpagedSystemMemorySize64 = process.NonpagedSystemMemorySize64,
            PeakPagedMemorySize64 = process.PeakPagedMemorySize64,
            PeakVirtualMemorySize64 = process.PeakVirtualMemorySize64,
            PeakWorkingSet64 = process.PeakWorkingSet64,
            PrivateMemorySize64 = process.PrivateMemorySize64,
            WorkingSet64 = process.WorkingSet64,
        });
    }

#region Config
    
    [HttpGet("system/config"), ShowInNavBar("Configuration")]
    public ActionResult Config() => View(new ConfigViewModel
    {
        Config = config
    });
    
    [HttpPost("system/config/save")]
    public ActionResult ConfigSave(CancellationToken ct) { config.Save(ct); return RedirectToAction(nameof(Config)); }
    
    [HttpPost("system/config/load")]
    public ActionResult ConfigLoad(CancellationToken ct) { config.Load(ct); return RedirectToAction(nameof(Config)); }

#endregion
#region No Category
    
    [HttpGet("ping")]
    public ActionResult Ping() => Ok();

    [HttpGet("system/npm-run-build"), ShowInNavBar]
    public ActionResult NpmRunBuild() {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            UseShellExecute = true,
            FileName = "npm",
            Arguments = "run build",
        }).WaitForExit();
        return View(new IndexViewModel
        {
        });
    }
    
    // List of libraries, licenses, materials used (images, etc)
    [HttpGet("system/credits"), ShowInNavBar("Credits")]
    public ActionResult Credits() => View(new CreditsViewModel
    {
    });
    
#endregion
#region Scheduled Tasks
    
    // List of scheduled tasks
    [HttpGet("system/tasks"), ShowInNavBar("Tasks")]
    public ActionResult ScheduledTasks() => View(new ScheduledTasksViewModel
    {
        Tasks = scheduledTaskManager.GetScheduledTasks()
    });

    [HttpGet("system/tasks/details/{name}")]
    public ActionResult ScheduledTaskDetails(string name) => View(new ScheduledTasksViewModel
    {
        Task = scheduledTaskManager.GetScheduledTasks().Where(t => t.GetType().FullName == name).SingleOrDefault(),
        Contexts = scheduledTaskManager.GetScheduledTaskContextsForTask(name),
        RunUrl = Url.Action(nameof(RunScheduledTask), new { name }),
        QueueUrl = Url.Action(nameof(QueueScheduledTask), new { name }),
    });

    [HttpPost("system/tasks/run/{name}")]
    public async Task<ActionResult> RunScheduledTask(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        var t = scheduledTaskManager.GetScheduledTask(name) ?? throw new Exception($"Could not find task {name}");
        var progress = (await scheduledTaskManager.Execute(t, asynchronous: false)) ?? throw new Exception($"Could not execute {name}");
        return RedirectToAction(nameof(ScheduledTaskContextDetails), new { id = progress.Id });
    }

    [HttpPost("system/tasks/queue/{name}")]
    public async Task<ActionResult> QueueScheduledTask(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        var t = scheduledTaskManager.GetScheduledTask(name) ?? throw new Exception($"Could not find task {name}");
        var progress = (await scheduledTaskManager.Execute(t)) ?? throw new Exception($"Could not queue {name}");
        return RedirectToAction(nameof(ScheduledTaskContextDetails), new { id = progress.Id });
    }

    [HttpGet("system/tasks/contexts/details/{id}")]
    public ActionResult ScheduledTaskContextDetails(Guid id) => View(new ScheduledTasksViewModel
    {
        TaskContext = scheduledTaskManager.GetContext(id),
    });

#endregion
#region Document Providers

    [HttpGet("system/document-providers"), ShowInNavBar("Document Providers")]
    public async Task<ActionResult> DocumentProviders() => View(new DocumentProvidersViewModel
    {
        ReadOnly = await documentProviderResolver.AllReadonlyProviders,
        Readwrite = await documentProviderResolver.AllReadWriteProviders,
    });

    [HttpGet("system/local-network-document-providers"), ShowInNavBar("Local Network Document Providers")]
    public async Task<ActionResult> LocalNetworkDocumentProviders() => View(new DocumentProvidersViewModel
    {
        ReadOnly = await documentProviderResolver.ReadRemotes,
        Readwrite = await documentProviderResolver.ReadWriteRemotes,
    });

#endregion
#region Hosts

    [HttpGet("system/hosts"), ShowInNavBar("Hosts")]
    public async Task<ActionResult> Hosts() => View(new HostsViewModel
    {
        AllHosts = await Task.WhenAll(slowPokeHostProvider.All.Select(GetViewModelForHost)),
        TrustedHosts = await Task.WhenAll(slowPokeHostProvider.Trusted.Select(GetViewModelForHost)),
        KnownButUntrustedHosts = await Task.WhenAll(slowPokeHostProvider.KnownButUntrusted.Select(GetViewModelForHost)),
    });

    private async Task<SlowPokeHostViewModel> GetViewModelForHost(ISlowPokeHost host)
    {
        return new SlowPokeHostViewModel(host, await slowPokeHostProvider.OpenClient(host.Endpoint, CancellationToken.None));
    }

    // initiates a search request
    [HttpGet("system/hosts/search")]
    public async Task<ActionResult> HostsSearch(CancellationToken ct) =>
        RedirectToAction(nameof(ScheduledTaskContextDetails),
                            new { id = (await scheduledTaskManager.Execute(scheduledTaskManager.GetScheduledTask(typeof(ScanLocalNetworkForPeersScheduledTask).FullName))).Id });

    [HttpGet("system/hosts/details/{endpoint}")]
    public async Task<ActionResult> HostDetails(string endpoint, CancellationToken cancellationToken)
    {
        if (Uri.TryCreate(Uri.UnescapeDataString(endpoint), new UriCreationOptions(), out var uri))
        {
            var host = await slowPokeHostProvider.GetHost(uri, cancellationToken);
            var client = await slowPokeHostProvider.OpenClient(uri, CancellationToken.None);
            return View(new HostsViewModel
            {
                Host = new SlowPokeHostViewModel(host, client)
            });
        }
        else
        {
            return BadRequest();
        }
    }

#endregion
#region Broadcast Messages

    [HttpGet("system/mem-cached-broadcast-msgs"), ShowInNavBar("Memory Cached Broadcast Messages")]
    public ActionResult MemoryCachedBroadcastMessages() => View();

    [HttpGet("system/disk-cached-broadcast-msgs"), ShowInNavBar("Disk Cached Broadcast Messages")]
    public ActionResult DiskCachedBroadcastMessages() => View();

#endregion
#region Identites and Authentication

    [HttpGet("system/identity-authentication"), ShowInNavBar("Identites and Authentication")]
    public ActionResult IdentityAuthentication() => View();

    [HttpPost("system/identity-authentication/trust/{id}")]
    public ActionResult IdentityAuthenticationTrust(Guid id)
    {
        return SetTrustLevel(id, TrustLevel.Trusted);
    }

    [HttpPost("system/identity-authentication/untrust-known/{id}")]
    public ActionResult IdentityAuthenticationUntrustKnown(Guid id)
    {
        return SetTrustLevel(id, TrustLevel.KnownButUntrusted);
    }

    [HttpPost("system/identity-authentication/forget/{id}")]
    public ActionResult IdentityAuthenticationForget(Guid id)
    {
        return SetTrustLevel(id, TrustLevel.Unknown);
    }

    private ActionResult SetTrustLevel(Guid id, TrustLevel trustLevel)
    {
        var ct = System.Threading.CancellationToken.None;
        var identity = IdentityAuthenticationService.GetIdentityFromAuthGuid(id, ct);
        if (identity == null)
            return NotFound();
        IdentityAuthenticationService.SetTrustLevel(identity, trustLevel, ct);
        return RedirectToAction(nameof(IdentityAuthentication));
    }
#endregion
}