using Leftware.Common;
using Spectre.Console;

namespace Leftware.Tasks.Core.TaskParameters.ConsoleReaders;

internal class ReadFolderTaskParameterConsoleReader : TaskParameterConsoleReaderBase<ReadFolderTaskParameter>
{
    public override void Read(ConsoleReadContext context, ReadFolderTaskParameter param)
    {
        if (AskIfDefaultValue(context, param)) return;

        var labelForPrompt = GetLabelForPrompt(param);
        string input;
        while (true)
        {
            var prompt = new TextPrompt<string>(labelForPrompt);
            input = AnsiConsole.Prompt(prompt);
            if (input == param.CancelString)
            {
                context.IsCanceled = true;
                return;
            }

            try
            {
                input = input.Trim();
                var dir = Path.GetDirectoryName(input);
                if (!Directory.Exists(dir))
                {
                    UtilConsole.WriteError("Directory path not found");
                    continue;
                }
                if (param.ShouldExist && !Directory.Exists(input))
                {
                    UtilConsole.WriteError("Directory not found");
                    continue;
                }

            }
            catch (Exception)
            {
                UtilConsole.WriteError("Error trying to interpret directory");
                continue;
            }

            break;
        }

        if (string.IsNullOrEmpty(input))
        {
            context.IsCanceled = true;
            return;
        }

        context[param.Name] = input;
    }
}
