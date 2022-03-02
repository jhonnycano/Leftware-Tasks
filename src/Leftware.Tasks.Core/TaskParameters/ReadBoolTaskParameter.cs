namespace Leftware.Tasks.Core.TaskParameters;

public class ReadBoolTaskParameter : TaskParameter<bool>
{
    public ReadBoolTaskParameter(string name, string label) : base(name, label)
    {
        Type = TaskParameterType.ReadBool;
    }

    public bool DefaultValue { get; private set; }

    public ReadBoolTaskParameter WithDefaultValue(bool defaultValue)
    {
        DefaultValue = defaultValue;
        return this;
    }
}
