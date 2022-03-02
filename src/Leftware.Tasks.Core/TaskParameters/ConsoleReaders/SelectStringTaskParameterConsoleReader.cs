using Spectre.Console;

namespace Leftware.Tasks.Core.TaskParameters.ConsoleReaders;

internal class SelectStringTaskParameterConsoleReader : TaskParameterConsoleReaderBase<SelectStringTaskParameter>
{
    public override void Read(ConsoleReadContext context, SelectStringTaskParameter param)
    {
        var sourceList = new List<string>(param.List);
        sourceList.Add(CANCEL_LABEL);

        var value = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .Title($"[green]{param.Label}[/]")
            .AddChoices(sourceList)
            );

        if (value == CANCEL_LABEL)
        {
            context.IsCanceled = true;
            return;
        }

        context[param.Name] = value;

        /*
        var value = UtilConsole.SelectFromList(param.List, param.Label, param.ExitValue, param.DefaultValue);
        if (string.IsNullOrEmpty(value) && !param.AllowEmpty)
        {
            context.IsCanceled = true;
            return;
        }

        context[param.Name] = value;
        */
    }
}
