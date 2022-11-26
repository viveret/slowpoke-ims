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
using SlowPokeIMS.Integration.Tests.Core;
using slowpoke.core.Services.Identity;

namespace SlowPokeIMS.Core.Tests.Services.Scheduled;

public class ScheduledTaskManagerIntegrationTests: IClassFixture<WebServerFixture>
{
    private readonly WebServerFixture fixture;

    public ScheduledTaskManagerIntegrationTests(WebServerFixture fixture)
    {
        this.fixture = fixture;
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task ScheduledTaskManager_Execute(bool asynchronous, bool immediately)
    {
        var cfg = new Config();
        var syncStateManager = new SyncStateManager();
        syncStateManager.ClearStaticVars();
        var authService = new StubIdentityAuthenticationService();
        var hostProvider = new TestSlowPokeHostProvider(cfg, fixture, authService);
        var broadcastProviderResolver = BroadcastProviderResolver.CreateForTesting(cfg, hostProvider);
        var readRemotes = new List<IReadOnlyDocumentResolver>() { };
        var readLocal = new ReadOnlyLocalDocumentResolver(cfg, broadcastProviderResolver);
        var resolver = new TestDocumentProviderResolver(readLocal, null, null, readRemotes, null, null, null);
        readRemotes.Add(new HttpReadOnlyRemoteDocumentResolver(await fixture.GetClient1(), cfg, resolver));
        var mgr = new ScheduledTaskManager(new ScheduledTaskContextStorage(), new IScheduledTask[] { new ScanLocalAndPullChangesScheduledTask(cfg, syncStateManager, null, resolver) });
        Assert.Single(mgr.Tasks);

        await mgr.Execute(mgr.GetScheduledTask(typeof(ScanLocalAndPullChangesScheduledTask).FullName!), asynchronous, immediately);
        
        if (asynchronous)
        {
            Assert.True(await mgr.HasQueuedTask());
            await mgr.ExecuteNextQueuedTask();
        }

        var contexts = mgr.GetScheduledTaskContextsForTask(typeof(ScanLocalAndPullChangesScheduledTask));
        Assert.Single(contexts);
        var result = contexts.Single();
        Assert.True(result.HasCompleted);
        Assert.Null(result.Error);
        Assert.NotEqual(DateTime.MinValue, result.WhenStarted);
        Assert.NotEqual(DateTime.MinValue, result.WhenCompleted);

        Assert.NotNull(result.SyncState);
        Assert.Null(result.SyncState.Error);
        Assert.False(result.SyncState.HasChangesToSend);
        Assert.False(result.SyncState.HasPolledChanges);
        Assert.False(result.SyncState.HasSentPublishedChanges);
        Assert.False(result.SyncState.IsUpToDateWithPolledChanges);

        // should be default / min value because not used
        Assert.Equal(SyncState.Uninitialized, result.SyncState.State);
        Assert.Equal(DateTime.MinValue, result.SyncState.LastTimePolledForChanges);
        Assert.Equal(DateTime.MinValue, result.SyncState.LastTimeSentPublishedChanges);
        Assert.Equal(DateTime.MinValue, result.SyncState.LastTimeStateChanged);

        Assert.Null(result.SyncState.Progress);
    }
}