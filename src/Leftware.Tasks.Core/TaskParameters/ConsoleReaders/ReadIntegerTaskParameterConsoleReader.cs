using Spectre.Console;

namespace Leftware.Tasks.Core.TaskParameters.ConsoleReaders;

internal class ReadIntegerTaskParameterConsoleReader : TaskParameterConsoleReaderBase<ReadIntegerTaskParameter>
{
    public override void Read(ConsoleReadContext context, ReadIntegerTaskParameter param)
    {
        if (AskIfDefaultValue(context, param)) return;

        var labelForPrompt = GetLabelForPrompt(param);

        var itemValue = AnsiConsole.Prompt(
            new TextPrompt<int?>(labelForPrompt)
            .Validate(n => n >= param.MinValue && n <= param.MaxValue)
            .AllowEmpty()
            );

        if (itemValue == null)
        {
            context.IsCanceled = true;
            return;
        }

        AddAndShow(context, param.Name, itemValue.Value);
    }
}
