using Spectre.Console;

namespace Leftware.Tasks.Core.TaskParameters.ConsoleReaders;

internal class SelectStringTaskParameterConsoleReader : TaskParameterConsoleReaderBase<SelectStringTaskParameter>
{
    public override void Read(ConsoleReadContext context, SelectStringTaskParameter param)
    {
        if (AskIfDefaultValue(context, param)) return;

        var labelForPrompt = GetLabelForPrompt(param);
        var sourceList = new List<string>(param.List);
        sourceList.Add(Defs.CANCEL_LABEL);

        var prompt = new SelectionPrompt<string>()
            .Title(labelForPrompt)
            .AddChoices(sourceList);
        var value = AnsiConsole.Prompt(prompt);
        if (value == Defs.CANCEL_LABEL)
        {
            context.IsCanceled = true;
            return;
        }

        context[param.Name] = value;
    }
}
