using Leftware.Common;
using Spectre.Console;

namespace Leftware.Tasks.Core.TaskParameters.ConsoleReaders;

internal class SelectEnumTaskParameterConsoleReader : TaskParameterConsoleReaderBase<SelectEnumTaskParameter>
{
    public override void Read(ConsoleReadContext context, SelectEnumTaskParameter param)
    {
        if (AskIfDefaultValue(context, param)) return;

        var labelForPrompt = GetLabelForPrompt(param);
        var options = PrepareOptions(param);
        var result = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .Title(labelForPrompt)
            .AddChoices(options)
            );

        if (result == Defs.CANCEL_LABEL)
        {
            context.IsCanceled = true;
            return;
        }

        var enumValue = UtilEnum.Get(param.EnumType, result);
        AddAndShow(context, param.Name, enumValue);
    }

    private static List<string> PrepareOptions(SelectEnumTaskParameter param)
    {
        var options = new List<string>(Enum.GetNames(param.EnumType))
        {
            Defs.CANCEL_LABEL
        };
        if (param.ValuesToSkip != null)
        {
            foreach (var valueToSkip in param.ValuesToSkip)
            {
                options.Remove(valueToSkip);
            }
        }

        return options;
    }
}
