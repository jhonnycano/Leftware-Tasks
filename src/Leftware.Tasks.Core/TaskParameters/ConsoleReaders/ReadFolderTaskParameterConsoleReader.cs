using Leftware.Common;
using Spectre.Console;

namespace Leftware.Tasks.Core.TaskParameters.ConsoleReaders;

internal class ReadFolderTaskParameterConsoleReader : TaskParameterConsoleReaderBase<ReadFolderTaskParameter>
{
    public override void Read(ConsoleReadContext context, ReadFolderTaskParameter param)
    {
        var labelToShow = $"[green]{param.Label}. [/]";
        AnsiConsole.Markup(labelToShow);
        if (param.DefaultValue != null)
            if (AnsiConsole.Confirm($"Use current value ({param.DefaultValue}). ?", true))
            {
                AddAndShow(context, param.Name, param.DefaultValue);
                return;
            }

        string input;
        while (true)
        {
            var prompt = new TextPrompt<string>("[blue] :>[/]");
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
            catch (Exception ex)
            {
                UtilConsole.WriteError("Error trying to interpret directory");
                continue;
            }

            break;
        }

        //var value = UtilConsole.ReadFile(param.Label, param.ShouldExist);
        if (string.IsNullOrEmpty(input))
        {
            context.IsCanceled = true;
            return;
        }

        context[param.Name] = input;

        /*
        var value = UtilConsole.ReadFolder(param.Label, param.ShouldExist);
        if (string.IsNullOrEmpty(value))
        {
            context.IsCanceled = true;
            return;
        }

        context[param.Name] = value;
        */
    }
}
