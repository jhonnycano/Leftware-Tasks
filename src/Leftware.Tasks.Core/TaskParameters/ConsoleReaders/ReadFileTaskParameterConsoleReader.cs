﻿using Leftware.Common;
using Spectre.Console;

namespace Leftware.Tasks.Core.TaskParameters.ConsoleReaders;

internal class ReadFileTaskParameterConsoleReader : TaskParameterConsoleReaderBase<ReadFileTaskParameter>
{
    public override void Read(ConsoleReadContext context, ReadFileTaskParameter param)
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
                    UtilConsole.WriteError("File path not found");
                    continue;
                }
                if (param.ShouldExist && !File.Exists(input))
                {
                    UtilConsole.WriteError("File not found");
                    continue;
                }

            }
            catch (Exception ex)
            {
                UtilConsole.WriteError("Error trying to interpret file");
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
    }
}
