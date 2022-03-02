using Spectre.Console;

namespace Leftware.Tasks.Core.TaskParameters.ConsoleReaders;

public abstract class TaskParameterConsoleReaderBase
{
    public const string MANUAL_INPUT_LABEL = "-- Input manual value";
    public const string CANCEL_LABEL = "-- Return to main menu";

    public abstract void Read(ConsoleReadContext context, TaskParameter param);
}

public abstract class TaskParameterConsoleReaderBase<T> : TaskParameterConsoleReaderBase where T : TaskParameter
{
    public override void Read(ConsoleReadContext context, TaskParameter param)
    {
        Read(context, (T)param);
    }

    public abstract void Read(ConsoleReadContext context, T param);

    protected T AddAndShow<T>(ConsoleReadContext ctx, string key, T value)
    {
        ctx[key] = value;
        AnsiConsole.MarkupLine($":left_arrow: [yellow]{value}[/]");
        return value;
    }
}
