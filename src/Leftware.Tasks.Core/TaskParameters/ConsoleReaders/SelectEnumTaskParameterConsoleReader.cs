using Leftware.Common;
using Spectre.Console;

namespace Leftware.Tasks.Core.TaskParameters.ConsoleReaders;

internal class SelectEnumTaskParameterConsoleReader : TaskParameterConsoleReaderBase<SelectEnumTaskParameter>
{
    public override void Read(ConsoleReadContext context, SelectEnumTaskParameter param)
    {
        var options = new List<string>(Enum.GetNames(param.EnumType))
        {
            CANCEL_LABEL
        };
        if (param.ValuesToSkip != null)
        {
            foreach(var valueToSkip in param.ValuesToSkip)
            {
                options.Remove(valueToSkip);
            }
        }
        var result = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .Title($"[green]{param.Label}[/]")
            .AddChoices(options)
            );

        if (result == CANCEL_LABEL)
        {
            context.IsCanceled = true;
            return;
        }

        var enumValue = UtilEnum.Get(param.EnumType, result);
        AddAndShow(context, param.Name, enumValue);

        /*
        var value = UtilConsole.SelectFromEnum(param.Label, param.EnumType, param.DefaultValue);

        if (param.DefaultValue.Equals(value) && param.CancelIfDefault)
        {
            context.IsCanceled = true;
            return;
        }

        context[param.Name] = value;
        */
    }
}
