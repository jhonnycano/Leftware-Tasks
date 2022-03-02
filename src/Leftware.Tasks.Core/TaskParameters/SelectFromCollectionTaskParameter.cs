namespace Leftware.Tasks.Core.TaskParameters;

public class SelectFromCollectionTaskParameter : TaskParameter<string>
{
    public SelectFromCollectionTaskParameter(string name, string label, string collection,
        bool allowManualEntry = false) : base(name, label)
    {
        Type = TaskParameterType.SelectFromCollection;
        Collection = collection;
        AllowManualEntry = allowManualEntry;
        ExitValue = "?";
    }

    public string Collection { get; }
    public bool AllowManualEntry { get; set; }
    public string ExitValue { get; set; }
    public bool UseKeyAsValue { get; set; }
}
