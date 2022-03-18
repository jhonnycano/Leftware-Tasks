using Spectre.Console;

namespace Leftware.Tasks.Core;

public class TaskExecutionContext
{
    public bool SkipExecution { get; internal set; }
    public List<string> TasksToSkipInMacroRecord { get; internal set; }
    //public MacroHolder MacroHolder { get; set; }
    public IDictionary<string, object> ExtendedInfo { get; internal set; }
    public IList<CommonTaskHolder> HolderList { get; set; }
    public ICollectionProvider? CollectionProvider { get; set; }
    public ISettingsProvider? SettingsProvider { get; set; }
    public StatusContext? StatusContext { get; set; }

    public TaskExecutionContext()
    {
        TasksToSkipInMacroRecord = new List<string>();
        HolderList = new List<CommonTaskHolder>();
        ExtendedInfo = new Dictionary<string, object>();
        CollectionProvider = null;
        SettingsProvider = null;
    }
}