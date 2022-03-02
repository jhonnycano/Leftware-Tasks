namespace Leftware.Tasks.Core.TaskParameters;

public class SelectGroupTaskParameter : TaskParameter<object>
{
    public IList<TaskParameter> Parameters { get; }

    public SelectGroupTaskParameter(IList<TaskParameter> parameters) : base("", "")
    {
        Type = TaskParameterType.SelectGroup;
        Parameters = parameters;
    }

    public SelectGroupTaskParameter(params TaskParameter[] parameters) : base("", "")
    {
        Type = TaskParameterType.SelectGroup;
        Parameters = parameters;
    }
}
