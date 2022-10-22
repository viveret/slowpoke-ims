using System;

namespace SlowPokeIMS.Web.ViewModels.System;


public class IndexViewModel
{
    public string MachineName { get; set; }
    public string ProcessName { get; set; }
    public DateTime StartTime { get; set; }
    public int ThreadCount { get; set; }
    public long PagedMemorySize64 { get; set; }
    public long PagedSystemMemorySize64 { get; set; }
    public long NonpagedSystemMemorySize64 { get; set; }
    public long PeakPagedMemorySize64 { get; set; }
    public long PeakVirtualMemorySize64 { get; set; }
    public long PeakWorkingSet64 { get; set; }
    public long PrivateMemorySize64 { get; set; }
    public long WorkingSet64 { get; set; }
}