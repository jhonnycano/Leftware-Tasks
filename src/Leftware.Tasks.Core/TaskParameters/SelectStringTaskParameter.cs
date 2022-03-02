namespace Leftware.Tasks.Core.TaskParameters;

public class SelectStringTaskParameter : TaskParameter<string>
{
    public SelectStringTaskParameter(string name, string label, IList<string> list, bool allowEmpty = false) : base(name, label)
    {
        Type = TaskParameterType.SelectString;
        List = list;
        AllowEmpty = allowEmpty;
        ExitValue = "?";
    }

    public IList<string> List { get; }
    public bool AllowEmpty { get; set; }
    public string ExitValue { get; set; }
}
