using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using slowpoke.core.Models.Config;
using slowpoke.core.Models.Identity;
using slowpoke.core.Services.Identity;
using slowpoke.core.Services.Node.Docs;
using slowpoke.core.Services.Scheduled;
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

    public SystemController(
        Config config,
        IDocumentProviderResolver documentProviderResolver,
        IScheduledTaskManager scheduledTaskManager,
        IIdentityAuthenticationService identityAuthenticationService)
    {
        this.config = config;
        this.documentProviderResolver = documentProviderResolver;
        this.scheduledTaskManager = scheduledTaskManager;
        IdentityAuthenticationService = identityAuthenticationService;
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
    
    [HttpGet("system/config"), ShowInNavBar("Configuration")]
    public ActionResult Config() => View(new ConfigViewModel
    {
        Config = config
    });
    
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
    

    #region "Scheduled Tasks"
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
    public ActionResult RunScheduledTask(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        var t = scheduledTaskManager.GetScheduledTask(name) ?? throw new Exception($"Could not find task {name}");
        var progress = scheduledTaskManager.Execute(t, asynchronous: false) ?? throw new Exception($"Could not execute {name}");
        return RedirectToAction(nameof(ScheduledTaskContextDetails), new { id = progress.Id });
    }

    [HttpPost("system/tasks/queue/{name}")]
    public ActionResult QueueScheduledTask(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        var t = scheduledTaskManager.GetScheduledTask(name) ?? throw new Exception($"Could not find task {name}");
        var progress = scheduledTaskManager.Execute(t) ?? throw new Exception($"Could not queue {name}");
        return RedirectToAction(nameof(ScheduledTaskContextDetails), new { id = progress.Id });
    }

    [HttpGet("system/tasks/contexts/details/{id}")]
    public ActionResult ScheduledTaskContextDetails(Guid id) => View(new ScheduledTasksViewModel
    {
        TaskContext = scheduledTaskManager.GetContext(id),
    });
    #endregion

    [HttpGet("system/document-providers"), ShowInNavBar("Document Providers")]
    public ActionResult DocumentProviders() => View(new DocumentProvidersViewModel
    {
        ReadOnly = documentProviderResolver.AllReadonlyProviders,
        Readwrite = documentProviderResolver.AllReadWriteProviders,
    });

    [HttpGet("system/local-network-document-providers"), ShowInNavBar("Local Network Document Providers")]
    public ActionResult LocalNetworkDocumentProviders() => View(new DocumentProvidersViewModel
    {
        ReadOnly = documentProviderResolver.ReadRemotes,
        Readwrite = documentProviderResolver.ReadWriteRemotes,
    });

    [HttpGet("system/mem-cached-broadcast-msgs"), ShowInNavBar("Memory Cached Broadcast Messages")]
    public ActionResult MemoryCachedBroadcastMessages() => View();

    [HttpGet("system/disk-cached-broadcast-msgs"), ShowInNavBar("Disk Cached Broadcast Messages")]
    public ActionResult DiskCachedBroadcastMessages() => View();

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