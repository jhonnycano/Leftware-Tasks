namespace Leftware.Tasks.Core.TaskParameters.Conditions;

public abstract class TaskParameterCondition
{
    public TaskParameterConditionType Type { get; protected set; }
    public string Parameter { get; set; }

    protected TaskParameterCondition(string parameter)
    {
        Parameter = parameter ?? throw new InvalidOperationException(nameof(parameter));
    }

    abstract internal bool Evaluate(ConsoleReadContext context);
}
