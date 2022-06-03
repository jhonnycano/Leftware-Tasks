using Spectre.Console;
using System.Text.RegularExpressions;

namespace Leftware.Tasks.Core.TaskParameters.ConsoleReaders;

internal class ReadStringTaskParameterConsoleReader : TaskParameterConsoleReaderBase<ReadStringTaskParameter>
{
    public override void Read(ConsoleReadContext context, ReadStringTaskParameter param)
    {
        if (AskIfDefaultValue(context, param)) return;

        var labelForPrompt = GetLabelForPrompt(param);
        string input;
        while (true)
        {
            var prompt = new TextPrompt<string>(labelForPrompt);
            AddValidations(prompt, context, param);
            if (param.AllowEmptyValue) prompt.AllowEmpty();

            input = AnsiConsole.Prompt(prompt);
            if (string.IsNullOrWhiteSpace(input))
            {
                if (!param.AllowEmptyValue)
                {
                    var (l, t) = Console.GetCursorPosition();
                    Console.SetCursorPosition(l, t - 1);
                    AnsiConsole.Markup(labelForPrompt);
                    continue;
                }
                else
                    AddAndShow(context, param.Name, input);

                return;
            }

            if (input == param.CancelString)
            {
                context.IsCanceled = true;
                return;
            }

            break;
        }

        AddAndShow(context, param.Name, input);
    }

    private void AddValidations(TextPrompt<string> prompt, ConsoleReadContext context, ReadStringTaskParameter param)
    {
        prompt
            .Validate(s => ValidateMinLength(s, param), "Value too short")
            .Validate(s => ValidateMaxLength(s, param), "Value too long")
            .Validate(s => ValidateRegex(s, param), "Value does not conform to expression");

        if (param.Validations != null)
        {
            foreach (var v in param.Validations)
            {
                prompt.Validate(s => CancellableValidator(s, param, v.validator), v.message);
            }
        }
        if (param.ReadContextValidations != null)
        {
            foreach (var v in param.ReadContextValidations)
            {
                prompt.Validate(s => CancellableValidator(s, param, context, v.validator), v.message);
            }
        }
    }

    private bool ValidateMinLength(string s, ReadStringTaskParameter param)
    {
        return s == param.CancelString || s?.Length > param.MinLength;
    }

    private bool ValidateMaxLength(string s, ReadStringTaskParameter param)
    {
        return s == param.CancelString || s?.Length < param.MaxLength;
    }

    private bool ValidateRegex(string s, ReadStringTaskParameter param)
    {
        return s == param.CancelString || (param.RegularExpression == null || (s != null && Regex.IsMatch(s, param.RegularExpression)));
    }
}
