namespace Leftware.Tasks.Core.TaskParameters.Conditions;

public class EqualsCondition : TaskParameterCondition
{
    public EqualsCondition(string parameter, object value) : base(parameter)
    {
        Type = TaskParameterConditionType.Equals;
        Value = value ?? throw new InvalidOperationException(nameof(value));
    }

    public object Value { get; set; }

    internal override bool Evaluate(ConsoleReadContext context)
    {
        return context.Values.ContainsKey(Parameter) && Value.Equals(context.Values[Parameter]);
    }
}
