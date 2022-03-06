namespace Leftware.Tasks.Core.TaskParameters;

public class ReadFolderTaskParameter : TaskParameter<string>
{
    public ReadFolderTaskParameter(string name, string label) : base(name, label)
    {
        Type = TaskParameterType.ReadFolder;
        ShouldExist = true;
    }

    public bool ShouldExist { get; private set; }
}
