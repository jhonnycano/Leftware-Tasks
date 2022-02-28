namespace Leftware.Tasks.Core;

public class TaskExecutionContext
{
    public bool SkipExecution { get; set; }
    public List<string> TasksToSkipInMacroRecord { get; set; }
    //public MacroHolder MacroHolder { get; set; }
    public IList<CommonTaskHolder> HolderList { get; set; }
    public IDictionary<string, object> ExtendedInfo { get; internal set; }

    public TaskExecutionContext()
    {
        TasksToSkipInMacroRecord = new List<string>();
        HolderList = new List<CommonTaskHolder>();
        ExtendedInfo = new Dictionary<string, object>();
    }
}