using System.Collections.Generic;
using slowpoke.core.Models.Config;
using slowpoke.core.Services.Scheduled;

namespace SlowPokeIMS.Web.ViewModels.System;


public class ScheduledTasksViewModel
{
    public IEnumerable<IScheduledTask> Tasks { get; set; }
    
    public bool IsCurrentlyRunning { get; set; }
    
    public IScheduledTask Task { get; set; }

    public string RunUrl { get; set; }
    
    public string QueueUrl { get; set; }
    
    public IEnumerable<IScheduledTaskContext> Contexts { get; set; }
    
    public IScheduledTaskContext TaskContext { get; set; }
}