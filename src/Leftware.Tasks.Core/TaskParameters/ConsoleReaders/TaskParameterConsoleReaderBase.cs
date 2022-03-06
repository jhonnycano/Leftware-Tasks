using Spectre.Console;

namespace Leftware.Tasks.Core.TaskParameters.ConsoleReaders;

public abstract class TaskParameterConsoleReaderBase
{
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
        AnsiConsole.MarkupLine($":left_arrow: [blue]{key}: [/] [yellow]{value}[/]");
        return value;
    }

    public bool AskIfDefaultValue(ConsoleReadContext context, IHasDefault param)
    {
        if (param.DefaultValue == null) return false;

        var defaultValueToShow = param.DefaultValueLabel ?? param.DefaultValue.ToString();
        var defaultLabel = $"Use default value ({defaultValueToShow})?";
        var label = $"[green]{param.Label}[/]. {defaultLabel}";
        if (AnsiConsole.Confirm(label, true))
        {
            AddAndShow(context, param.Name, param.DefaultValue);
            return true;
        }
        return false;
    }

    protected static string GetLabelForPrompt(TaskParameter param)
    {
        var formattedLabel = $"[green]{param.Label}. [/]";
        var promptString = $"[blue] :>[/]";
        var labelForPrompt = $"{formattedLabel}{promptString}";
        return labelForPrompt;
    }
}
