namespace Leftware.Tasks.Core.TaskParameters.Conditions;

public class ExistsCondition : TaskParameterCondition
{
    public bool Exists { get; set; }

    public ExistsCondition(string parameter, bool exists = true) : base(parameter)
    {
        Type = TaskParameterConditionType.Exists;
        Exists = exists;
    }

    internal override bool Evaluate(ConsoleReadContext context)
    {
        return Exists == context.Values.ContainsKey(Parameter);
    }
}
