using Leftware.Common;
using Spectre.Console;

namespace Leftware.Tasks.Core.TaskParameters.ConsoleReaders;

internal class ReadPasswordTaskParameterConsoleReader : TaskParameterConsoleReaderBase<ReadPasswordTaskParameter>
{
    public override void Read(ConsoleReadContext context, ReadPasswordTaskParameter param)
    {
        string value;
        while (true)
        {
            //value = UtilConsole.ReadPassword(param.Label, param.CharMask);
            var labelToShow = $"[green]{param.Label}. [/]";
            value = AnsiConsole.Prompt(new TextPrompt<string>(labelToShow).Secret());
            if (string.IsNullOrEmpty(value) || param.CancelString.Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                context.IsCanceled = true;
                return;
            }

            if (value.Length < param.MinLength)
            {
                UtilConsole.WriteWarning("Password length too short");
                continue;
            }
            if (value.Length > param.MaxLength)
            {
                UtilConsole.WriteWarning("Password length too long");
                continue;
            }

            break;
        }

        context[param.Name] = value;
    }
}
