using Leftware.Tasks.Core.TaskParameters.Conditions;

namespace Leftware.Tasks.Core.TaskParameters;

public enum TaskParameterType
{
    None,
    ReadBool,
    ReadInteger,
    ReadString,
    ReadPassword,
    ReadFile,
    ReadFolder,
    SelectString,
    SelectEnum,
    SelectFromCollection,
    SelectGroup,
    CustomConsole,
}

public abstract class TaskParameter
{
    public TaskParameterType Type { get; protected set; }

    public string Name { get; set; }

    public string Label { get; set; }

    public IList<TaskParameterCondition> Conditions { get; }

    public TaskParameter(string name, string label)
    {
        Name = name;
        Label = label;
        Conditions = new List<TaskParameterCondition>();
    }
}

public abstract class TaskParameter<T> : TaskParameter
{
    protected TaskParameter(string name, string label) : base(name, label)
    {
        Name = name;
        Label = label;
        DefaultValue = default;
    }

    public T? DefaultValue { get; set; }

    public TaskParameter<T> When(TaskParameterCondition condition)
    {
        Conditions.Add(condition);
        return this;
    }

    public TaskParameter<T> WithDefaultValue(T defaultValue)
    {
        DefaultValue = defaultValue;
        return this;
    }
}
