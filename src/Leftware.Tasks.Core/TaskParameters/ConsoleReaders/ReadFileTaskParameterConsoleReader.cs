﻿using Leftware.Common;
using Spectre.Console;

namespace Leftware.Tasks.Core.TaskParameters.ConsoleReaders;

internal class ReadFileTaskParameterConsoleReader : TaskParameterConsoleReaderBase<ReadFileTaskParameter>
{
    public override void Read(ConsoleReadContext context, ReadFileTaskParameter param)
    {
        if (AskIfDefaultValue(context, param)) return;

        var labelForPrompt = GetLabelForPrompt(param);
        string input;
        while (true)
        {
            var prompt = new TextPrompt<string>(labelForPrompt);
            AddValidations(prompt, context, param);
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

        if (string.IsNullOrEmpty(input))
        {
            context.IsCanceled = true;
            return;
        }

        context[param.Name] = input;
    }

    private void AddValidations(TextPrompt<string> prompt, ConsoleReadContext context, ReadFileTaskParameter param)
    {
        if (param.Validations != null)
        {
            foreach (var (validator, message) in param.Validations)
            {
                prompt.Validate(s => CancellableValidator(s, param, validator), message);
            }
        }
    }
}
