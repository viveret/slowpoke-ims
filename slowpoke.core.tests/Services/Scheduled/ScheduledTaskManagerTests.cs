using System;
using Xunit;

using slowpoke.core.Services;
using slowpoke.core.Models.SyncState;
using System.Linq;
using slowpoke.core.Services.Scheduled;
using slowpoke.core.Services.Scheduled.Tasks;
using slowpoke.core.Services.Node.Docs;
using slowpoke.core.Models.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;
using slowpoke.core.Models.Node;
using slowpoke.core.Services.Broadcast;
using slowpoke.core.Services.Node;
using SlowPokeIMS.Tests.Core.Services;
using slowpoke.core.Services.Http;

namespace SlowPokeIMS.Core.Tests.Services.Scheduled;

public class ScheduledTaskManagerTests
{
    [Fact]
    public void ScheduledTaskManager_Constructor_Default()
    {
        var mgr = new ScheduledTaskManager(new ScheduledTaskContextStorage(), Enumerable.Empty<IScheduledTask>());
    }
    
    [Fact]
    public void ScheduledTaskManager_Tasks_Empty()
    {
        var mgr = new ScheduledTaskManager(new ScheduledTaskContextStorage(), Enumerable.Empty<IScheduledTask>());
        Assert.Empty(mgr.Tasks);
    }
    
    [Fact]
    public void ScheduledTaskManager_Tasks_Single()
    {
        var cfg = new Config();
        var syncStateManager = new SyncStateManager();
        var resolver = new StubDocumentProviderResolver(cfg);
        var mgr = new ScheduledTaskManager(new ScheduledTaskContextStorage(), new IScheduledTask[] { new ScanLocalAndPullChangesScheduledTask(cfg, syncStateManager, null, resolver) });
        Assert.Single(mgr.Tasks);
    }
}