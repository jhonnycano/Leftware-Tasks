using Spectre.Console;
using System.Text.RegularExpressions;

namespace Leftware.Tasks.Core.TaskParameters.ConsoleReaders;

internal class ReadStringTaskParameterConsoleReader : TaskParameterConsoleReaderBase<ReadStringTaskParameter>
{
    public override void Read(ConsoleReadContext context, ReadStringTaskParameter param)
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

            AddValidations(prompt, context, param);
            if (param.AllowEmptyValue) prompt.AllowEmpty();

            input = AnsiConsole.Prompt(prompt);
            if (string.IsNullOrWhiteSpace(input))
            {
                if (!param.AllowEmptyValue)
                {
                    var (l, t) = Console.GetCursorPosition();
                    Console.SetCursorPosition(l, t - 1);
                    AnsiConsole.Markup(labelToShow);
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

    private bool CancellableValidator(string s, ReadStringTaskParameter param, Func<string, bool> validator)
    {
        return s == param.CancelString || validator(s);
    }

    private bool CancellableValidator(string s, ReadStringTaskParameter param, ConsoleReadContext ctx, Func<string, ConsoleReadContext, bool> validator)
    {
        return s == param.CancelString || validator(s, ctx);
    }
}
