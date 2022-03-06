namespace Leftware.Tasks.Core.TaskParameters;

public class ReadFileTaskParameter : TaskParameter<string>
{
    public ReadFileTaskParameter(string name, string label) : this(name, label, true)
    {
    }

    public ReadFileTaskParameter(string name, string label, bool shouldExist) : base(name, label)
    {
        Type = TaskParameterType.ReadFile;
        ShouldExist = shouldExist;
    }

    public bool ShouldExist { get; private set; }
}
