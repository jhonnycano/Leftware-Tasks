namespace Leftware.Tasks.Core.TaskParameters;

public class CustomConsoleTaskParameter : TaskParameter<object>
{
    public Func<ConsoleReadContext, bool> CustomReadFunction { get; set; }

    public CustomConsoleTaskParameter(Func<ConsoleReadContext, bool> func) : base("", "")
    {
        Type = TaskParameterType.CustomConsole;
        CustomReadFunction = func;
    }
}
