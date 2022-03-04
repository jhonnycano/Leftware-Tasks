namespace Leftware.Tasks.Core.TaskParameters;

public class ReadFolderTaskParameter : TaskParameter<string>
{
    public ReadFolderTaskParameter(string name, string label, bool shouldExist) : base(name, label)
    {
        Type = TaskParameterType.ReadFolder;
        ShouldExist = shouldExist;
    }

    public bool ShouldExist { get; set; }
    public string CancelString { get; internal set; }
}
