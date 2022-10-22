namespace slowpoke.core.Models.SyncState;

public enum SyncState
{
    ErrorState = 0, // an error occoured
    Unknown, // sync state not in a valid state (not specifically an error)
    Uninitialized, // sync has not yet checked for changes
    OutOfDate, // sync has checked for changes and received or has newer data
    UpToDate, // sync has checked for changes and there are no available newer changes and no new changes to broadcast
    Synchronizing, // sync is broadcasting changes, either receiving and/or sending updates
}

public class SyncStateTaskProgress
{
    public string Title { get; set; }
    
    public string Subtitle { get; set; }

    public long ProgressMax { get; set; }
    
    public long ProgressValue { get; set; }

    public List<SyncStateTaskProgress> ChildTasks { get; set; }
    
    public SyncStateTaskProgress ParentTask { get; set; }
}

public class SyncStateModel
{
    public bool HasChangesToSend { get; set; }

    public bool HasSentPublishedChanges { get; set; }
    
    public DateTime LastTimeSentPublishedChanges { get; set; } = DateTime.MinValue;
    
    public bool IsUpToDateWithPolledChanges { get; set; }
    
    public DateTime LastTimePolledForChanges { get; set; } = DateTime.MinValue;

    public bool HasPolledChanges => DateTime.MinValue < LastTimePolledForChanges;

    public SyncState State 
    {
        get
        {
            if (Error != null)
            {
                return SyncState.ErrorState;
            }
            else if (!HasPolledChanges)
            {
                return SyncState.Uninitialized;
            }
            else if (!HasChangesToSend)
            {
                return SyncState.Unknown;
            }
            else if (!HasSentPublishedChanges && !IsUpToDateWithPolledChanges)
            {
                return SyncState.OutOfDate;
            }
            else if (Progress != null && (Progress.ProgressValue < Progress.ProgressMax || Progress.ProgressMax == -1))
            {
                return SyncState.Synchronizing;
            }
            else if (HasSentPublishedChanges && IsUpToDateWithPolledChanges)
            {
                return SyncState.UpToDate;
            }
            else
            {
                return SyncState.ErrorState;
            }
        }
    }

    public DateTime LastTimeStateChanged { get; set; }

    public Exception Error { get; set; }

    public SyncStateTaskProgress Progress { get; set; }    
}