using System;
using Xunit;

using slowpoke.core.Services;
using slowpoke.core.Models.SyncState;

namespace SlowPokeIMS.Core.Tests.Services;

public class SyncStateManagerTests
{
    [Fact]
    public void SyncStateManager_Constructor_Default()
    {
        var mgr = new SyncStateManager();
    }
    
    [Fact]
    public void SyncStateManager_GetForSystem_Works()
    {
        var mgr = new SyncStateManager();
        Assert.NotNull(mgr.GetForSystem());
    }
    
    [Fact]
    public void SyncStateManager_SetForSystem_Works()
    {
        var mgr = new SyncStateManager();
        mgr.SetForSystem(new slowpoke.core.Models.SyncState.SyncStateModel { });
    }
    
    [Fact]
    public void SyncStateManager_SetForSystem_Null_Invalid()
    {
        var mgr = new SyncStateManager();
        Assert.Throws<ArgumentNullException>(() => mgr.SetForSystem(null));
    }
    
    [Fact]
    public void SyncStateManager_GetForAction_Random_Works()
    {
        var mgr = new SyncStateManager();
        mgr.GetForAction(Guid.NewGuid());
    }
    
    [Fact]
    public void SyncStateManager_SetForAction_Random_Works()
    {
        var mgr = new SyncStateManager();
        mgr.SetForAction(Guid.NewGuid(), new slowpoke.core.Models.SyncState.SyncStateModel { });
    }
    
    [Fact]
    public void SyncStateManager_SetForAction_Random_Null_Invalid()
    {
        var mgr = new SyncStateManager();
        Assert.Throws<ArgumentNullException>(() => mgr.SetForAction(Guid.NewGuid(), null));
    }
    
    [Fact]
    public void SyncStateManager_SetForAction_Persists_State()
    {
        var mgr = new SyncStateManager();
        var testGuid = Guid.NewGuid();

        var testModel = new slowpoke.core.Models.SyncState.SyncStateModel { };
        mgr.SetForAction(testGuid, testModel);

        var actualModel = mgr.GetForAction(testGuid);
        AssertEqual(testModel, actualModel);


        var testModel2 = new slowpoke.core.Models.SyncState.SyncStateModel { Progress = new slowpoke.core.Models.SyncState.SyncStateTaskProgress { ProgressValue = 30, ProgressMax = 100 } };
        mgr.SetForAction(testGuid, testModel2);

        var actualModel2 = mgr.GetForAction(testGuid);
        AssertEqual(testModel2, actualModel2);
        

        var testModel3 = new slowpoke.core.Models.SyncState.SyncStateModel { Progress = new slowpoke.core.Models.SyncState.SyncStateTaskProgress { ProgressValue = 99, ProgressMax = 100 } };
        mgr.SetForAction(testGuid, testModel3);

        var actualModel3 = mgr.GetForAction(testGuid);
        AssertEqual(testModel3, actualModel3);
    }

    private void AssertEqual(SyncStateModel testModel, SyncStateModel actualModel)
    {
        if (testModel == null)
        {
            Assert.Null(actualModel);
        }
        else
        {
            Assert.NotNull(actualModel);
            Assert.Equal(testModel.State, actualModel.State);
            Assert.Equal(testModel.HasChangesToSend, actualModel.HasChangesToSend);
            Assert.Equal(testModel.HasPolledChanges, actualModel.HasPolledChanges);
            Assert.Equal(testModel.HasSentPublishedChanges, actualModel.HasSentPublishedChanges);
            Assert.Equal(testModel.IsUpToDateWithPolledChanges, actualModel.IsUpToDateWithPolledChanges);
            Assert.Equal(testModel.LastTimePolledForChanges, actualModel.LastTimePolledForChanges);
            Assert.Equal(testModel.LastTimeSentPublishedChanges, actualModel.LastTimeSentPublishedChanges);
            Assert.Equal(testModel.LastTimeStateChanged, actualModel.LastTimeStateChanged);
            Assert.Equal(testModel.Error, actualModel.Error);
            
            if (testModel.Progress == null)
            {
                Assert.Null(actualModel.Progress);
            }
            else
            {
                Assert.NotNull(actualModel.Progress);
                Assert.Equal(testModel.Progress.ProgressMax, actualModel.Progress.ProgressMax);
                Assert.Equal(testModel.Progress.ProgressValue, actualModel.Progress.ProgressValue);
                Assert.Equal(testModel.Progress.Title, actualModel.Progress.Title);
                Assert.Equal(testModel.Progress.Subtitle, actualModel.Progress.Subtitle);
            }
        }
    }
}