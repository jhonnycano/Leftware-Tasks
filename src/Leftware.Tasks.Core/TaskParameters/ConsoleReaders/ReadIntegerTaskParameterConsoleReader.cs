using Spectre.Console;

namespace Leftware.Tasks.Core.TaskParameters.ConsoleReaders;

internal class ReadIntegerTaskParameterConsoleReader : TaskParameterConsoleReaderBase<ReadIntegerTaskParameter>
{
    public override void Read(ConsoleReadContext context, ReadIntegerTaskParameter param)
    {
        var labelToShow = $"[green]{param.Label}. [/]";
        AnsiConsole.Markup(labelToShow);
        if (param.CurrentValue != null)
            if (AnsiConsole.Confirm($"Use current value ({param.CurrentValue}). ?", true))
            {
                AddAndShow(context, param.Name, param.CurrentValue.Value);
                return;
            }

        var itemValue = AnsiConsole.Prompt(
            new TextPrompt<int?>($"[blue] [[{param.MinValue}-{param.MaxValue}]]:>[/]")
            .Validate(n => n >= param.MinValue && n <= param.MaxValue)
            .AllowEmpty()
            );

        if (itemValue == null)
        {
            context.IsCanceled = true;
            return;
        }

        AddAndShow(context, param.Name, itemValue.Value);

        /*
        var value = UtilConsole.ReadInteger(param.Label, param.MinValue, param.MaxValue, param.DefaultValue);
        if (value == null)
        {
            context.IsCanceled = true;
            return;
        }

        context[param.Name] = value.Value;
        */
    }
}
