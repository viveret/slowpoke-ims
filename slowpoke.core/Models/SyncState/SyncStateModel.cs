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
    public SyncStateTaskProgress()
    {
    }

    public SyncStateTaskProgress(SyncStateTaskProgress other)
    {
        Title = other.Title;
        Subtitle = other.Subtitle;
        ProgressMax = other.ProgressMax;
        ProgressValue = other.ProgressValue;
    }

    public string Title { get; set; } = string.Empty;
    
    public string Subtitle { get; set; } = string.Empty;

    public long ProgressMax { get; set; }
    
    public long ProgressValue { get; set; }

    public IEnumerable<SyncStateTaskProgress> ChildTasks { get; set; } = Enumerable.Empty<SyncStateTaskProgress>();
    
    public SyncStateTaskProgress? ParentTask { get; set; }


    public SyncStateTaskProgress Copy() => new SyncStateTaskProgress(this);
}

public class SyncStateModel
{
    public SyncStateModel()
    {

    }

    public SyncStateModel(SyncStateModel other)
    {
        this.HasChangesToSend = other.HasChangesToSend;
        this.HasSentPublishedChanges = other.HasSentPublishedChanges;
        this.LastTimeSentPublishedChanges = other.LastTimeSentPublishedChanges;
        this.IsUpToDateWithPolledChanges = other.IsUpToDateWithPolledChanges;
        this.LastTimePolledForChanges = other.LastTimePolledForChanges;
        this.LastTimeStateChanged = other.LastTimeStateChanged;
        this.Error = other.Error;
        this.Progress = other.Progress?.Copy();
    }

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

    public Exception? Error { get; set; }

    public SyncStateTaskProgress? Progress { get; set; }

    public virtual SyncStateModel Copy() => new SyncStateModel(this);
}