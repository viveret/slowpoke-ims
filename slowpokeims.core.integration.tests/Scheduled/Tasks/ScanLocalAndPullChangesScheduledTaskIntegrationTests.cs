using Xunit;

using slowpoke.core.Services;
using slowpoke.core.Models.SyncState;
using slowpoke.core.Services.Scheduled;
using slowpoke.core.Services.Scheduled.Tasks;
using slowpoke.core.Services.Node.Docs;
using slowpoke.core.Models.Configuration;
using System.Threading.Tasks;
using slowpoke.core.Services.Broadcast;
using SlowPokeIMS.Integration.Tests.Core;
using SlowPokeIMS.Tests.Core.Services;
using System.Collections.Generic;
using slowpoke.core.Services.Identity;
using SlowPokeIMS.Core.Services.Node.Docs.PathRules;
using System.Linq;

namespace SlowPokeIMS.Core.Tests.Services.Scheduled.Tasks;

public class ScanLocalAndPullChangesScheduledTaskIntegrationTests: IClassFixture<WebServerFixture>
{
    private readonly WebServerFixture fixture;

    public ScanLocalAndPullChangesScheduledTaskIntegrationTests(WebServerFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public async Task ScanLocalAndPullChangesScheduledTask_Execute()
    {
        var cfg = new Config();
        var authService = new StubIdentityAuthenticationService();
        var hostProvider = new TestSlowPokeHostProvider(cfg, fixture, authService);
        var broadcastProviderResolver = BroadcastProviderResolver.CreateForTesting(cfg, hostProvider);

        // todo: fix this, should not be local, should be test
        var pathRuleService = GenericPathRuleService.Empty(cfg);
        var readLocal = new ReadOnlyLocalDocumentResolver(cfg, broadcastProviderResolver, pathRuleService) {};
        var writeLocal = new WritableLocalDocumentResolver(cfg, broadcastProviderResolver, pathRuleService) {};

        var readOnlyLocalDriveProviders = new IReadOnlyDocumentResolver[] {};
        var readRemotes = new List<IReadOnlyDocumentResolver>();
        var readWriteRemotes = new IWritableDocumentResolver[] {};
        var allReadonlyProviders = new IReadOnlyDocumentResolver[] {};
        var allReadWriteProvider = new IWritableDocumentResolver[] {};

        var docProviderResolver = new TestDocumentProviderResolver(readLocal, writeLocal,
                                        readOnlyLocalDriveProviders, readRemotes, readWriteRemotes,
                                        allReadonlyProviders, allReadWriteProvider);

        readRemotes.Add(new HttpReadOnlyRemoteDocumentResolver(await fixture.GetClient1(), cfg, docProviderResolver));

        var task = new ScanLocalAndPullChangesScheduledTask(cfg, new SyncStateManager(), null, docProviderResolver);
        var ctx = task.CreateContext(ScheduledTaskManager.Empty());
        Assert.NotNull(ctx);
        Assert.False(ctx.HasCompleted);
        Assert.Null(ctx.Error);
        Assert.NotNull(ctx.SyncState);
        Assert.Null(ctx.SyncState.Error);
        Assert.False(ctx.SyncState.HasChangesToSend);
        Assert.False(ctx.SyncState.HasPolledChanges);
        Assert.False(ctx.SyncState.HasSentPublishedChanges);
        Assert.False(ctx.SyncState.IsUpToDateWithPolledChanges);
        Assert.Equal(SyncState.Uninitialized, ctx.SyncState.State);
        Assert.Null(ctx.SyncState.Progress);

        await task.OnStart(ctx);
        await task.Execute(ctx);
        await task.OnEnd(ctx);

        Assert.True(ctx.HasCompleted);
        Assert.Null(ctx.Error);
        Assert.NotNull(ctx.SyncState);
        Assert.Null(ctx.SyncState.Error);
        Assert.False(ctx.SyncState.HasChangesToSend);
        Assert.False(ctx.SyncState.HasPolledChanges);
        Assert.False(ctx.SyncState.HasSentPublishedChanges);
        Assert.False(ctx.SyncState.IsUpToDateWithPolledChanges);
        Assert.Equal(SyncState.Uninitialized, ctx.SyncState.State);
        Assert.Null(ctx.SyncState.Progress);
    }
}