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

    public string CancelString { get; set; }
    
    public IList<TaskParameterCondition> Conditions { get; }

    public TaskParameter(string name, string label)
    {
        Name = name;
        Label = label;
        CancelString = Defs.DEFAULT_CANCEL_STRING;
        Conditions = new List<TaskParameterCondition>();
    }

    public TaskParameter WithCancelString(string cancelString)
    {
        CancelString = cancelString;
        return this;
    }
}

public abstract class TaskParameter<T> : TaskParameter, IHasDefault<T>
{
    protected TaskParameter(string name, string label) : base(name, label)
    {
        Name = name;
        Label = label;
        DefaultValue = default;
    }

    public T? DefaultValue { get; private set; }
    public string? DefaultValueLabel { get; private set; }
    object? IHasDefault.DefaultValue => DefaultValue;

    public TaskParameter<T> When(TaskParameterCondition condition)
    {
        Conditions.Add(condition);
        return this;
    }

    public TaskParameter<T> WithDefaultValue(T defaultValue, string? defaultValueLabel = null)
    {
        DefaultValue = defaultValue;
        DefaultValueLabel = defaultValueLabel;
        return this;
    }
}

public interface IHasDefault
{
    string Name { get; }
    string Label { get; }
    object? DefaultValue { get; }
    string? DefaultValueLabel { get; }
}

public interface IHasDefault<T> : IHasDefault
{
    new T? DefaultValue => DefaultValue;

}
