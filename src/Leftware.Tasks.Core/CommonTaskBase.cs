using Leftware.Tasks.Core.Model;
using Spectre.Console;
using System.Text.RegularExpressions;

namespace Leftware.Tasks.Core;

public abstract class CommonTaskBase
{
    private const string MANUAL_INPUT_LABEL = "-- Input manual value";
    private const string CANCEL_LABEL = "-- Return to main menu";
    public TaskExecutionContext Context { get; set; }

    public async virtual Task<IDictionary<string, object>?> GetTaskInput()
    {
        return await Task.FromResult(default(IDictionary<string, object>));
    }

    public abstract Task Execute(IDictionary<string, object> input);

    protected static IDictionary<string, object> GetEmptyTaskInput()
    {
        return new Dictionary<string, object>();
    }

    protected bool GetString(IDictionary<string, object> dic, string key, string label, 
    string? currentValue = null)
    {
        var labelToShow = $"[green]{label}. [/]";
        AnsiConsole.Markup(labelToShow);
        if (currentValue != null)
        {
            if (AnsiConsole.Confirm($"Use current value ({currentValue}). ?", true))
            {
                AddAndShow(dic, key, currentValue);
                return true;
            }
        }

        string itemValue;
        itemValue = AnsiConsole.Prompt(
            new TextPrompt<string>("[blue] :>[/]")
            .AllowEmpty()
            );

        if (string.IsNullOrWhiteSpace(itemValue)) return false;

        AddAndShow(dic, key, itemValue);
        return true;
    }

    protected bool GetStringFromCollection(IDictionary<string, object> dic, string key, string label, string collection,
        string? currentValue = null)
    {
        var labelToShow = $"[green]{label}. [/]";
        AnsiConsole.Markup(labelToShow);
        if (currentValue != null)
        {
            if (AnsiConsole.Confirm($"Use current value ({currentValue}). ?", true))
            {
                AddAndShow(dic, key, currentValue);
                return true;
            }
        }

        string itemValue;
        var items = Context!.CollectionProvider
            .GetItems(collection)
            .Select(i => i.Label)
            .Append(MANUAL_INPUT_LABEL)
            .Append(CANCEL_LABEL)
            .ToList();
        AnsiConsole.WriteLine();
        itemValue = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .AddChoices(items)
            );
        if (itemValue == CANCEL_LABEL) return false;
        if (itemValue == MANUAL_INPUT_LABEL)
        {
            itemValue = AnsiConsole.Prompt(
                new TextPrompt<string>("[blue] :>[/]")
                );

            if (string.IsNullOrWhiteSpace(itemValue)) return false;
        }

        AddAndShow(dic, key, itemValue);
        return true;
    }

    protected bool GetStringValidRegex(IDictionary<string, object> dic, string key, string label,
    string? currentValue = null,
    string? regex = null)
    {
        var labelToShow = $"[green]{label}. [/]";
        AnsiConsole.Markup(labelToShow);
        if (currentValue != null)
        {
            if (AnsiConsole.Confirm($"Use current value ({currentValue}). ?", true))
            {
                AddAndShow(dic, key, currentValue);
                return true;
            }
        }

        string itemValue;
        if (regex != null)
        {
            itemValue = AnsiConsole.Prompt(
                new TextPrompt<string>("[blue] :>[/]")
                .Validate(o => IsValidRegexOrEmpty(o, regex))
                .AllowEmpty()
                );
        }
        else
        {
            itemValue = AnsiConsole.Prompt(
                new TextPrompt<string>("[blue] :>[/]")
                .AllowEmpty()
                );
        }
        
        if (string.IsNullOrWhiteSpace(itemValue)) return false;

        AddAndShow(dic, key, itemValue);
        return true;
    }

    protected bool GetExistingFile(IDictionary<string, object> dic, string key, string label,
        string? currentValue = null
        )
    {
        var labelToShow = $"[green]{label}. [/]";
        AnsiConsole.Markup(labelToShow);
        if (currentValue != null)
        {
            if (AnsiConsole.Confirm($"Use current value ({currentValue}). ?", true))
            {
                AddAndShow(dic, key, currentValue);
                return true;
            }
        }

        var itemValue = AnsiConsole.Prompt(
            new TextPrompt<string>("[blue] :>[/]")
            .Validate(IsExistingFileOrEmpty, "Invalid file or not found")
            .AllowEmpty()
            );

        return !string.IsNullOrWhiteSpace(itemValue);
    }

    protected bool GetJson(IDictionary<string, object> dic, string key, string label, 
        string? currentValue = null, 
        string? schema = null)
    {
        var labelToShow = $"[green]{label}. [/]";
        AnsiConsole.Markup(labelToShow);
        if (currentValue != null)
        {
            if (AnsiConsole.Confirm($"Use current value ({currentValue}). ?", true))
            {
                AddAndShow(dic, key, currentValue);
                return true;
            }
        }

        string itemValue;
        if (schema != null)
        {
            itemValue = AnsiConsole.Prompt(
                new TextPrompt<string>(labelToShow)
                .Validate(s => IsValidJsonForSchema(s, schema), "json does not conform to the schema")
                .AllowEmpty()
                );
        }
        else
        {
            itemValue = AnsiConsole.Prompt(
                new TextPrompt<string>("[blue] :>[/]")
                .AllowEmpty()
                );
        }

        if (string.IsNullOrWhiteSpace(itemValue)) return false;

        AddAndShow(dic, key, itemValue);
        return true;
    }

    protected CollectionItem? GetItem(IDictionary<string, object> dic, string key, string label, string collection)
    {
        var items = Context.CollectionProvider.GetItems(collection);
        if (items.Count == 0)
        {
            AnsiConsole.MarkupLine($"[yellow]Empty collection[/] [magenta]{collection}[/]. Please add first some items to the collection before executing this task");
            return null;
        }

        var itemLabels = items.Select(i => i.Label).Append(CANCEL_LABEL);

        var labelToShow = $"[green]{label}. [/]";
        AnsiConsole.Markup(labelToShow);

        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .AddChoices(itemLabels)
            );
        var result = items.FirstOrDefault(i => i.Label == selection);
        if (result == null) return null;

        AddAndShow(dic, key, result.Key);
        return result;
    }

    protected string? SelectOption(IDictionary<string, object> dic, string key, string label, IEnumerable<string> options)
    {
        var labelToShow = $"[green]{label}. [/]";
        AnsiConsole.Markup(labelToShow);
        var newOptions = options.Append(CANCEL_LABEL);
        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .AddChoices(newOptions)
            );
        if (selection == null || selection == CANCEL_LABEL) return null;

        AddAndShow(dic, key, selection);
        return selection;
    }

    private bool IsValidRegexOrEmpty(string arg, string regex)
    {
        if (string.IsNullOrWhiteSpace(arg)) return true;
        return Regex.IsMatch(arg, regex);
    }

    private bool IsExistingFileOrEmpty(string arg)
    {
        if (string.IsNullOrWhiteSpace(arg)) return true;
        try
        {
            if (!File.Exists(arg)) return false;
        }
        catch (Exception)
        {
            return false;
        }
        return true;
    }

    private bool IsValidJsonForSchema(string json, string schemaJson)
    {
        return UtilJsonSchema.IsValidJsonForSchema(json, schemaJson);
    }

    private string AddAndShow(IDictionary<string, object> dic, string key, string value)
    {
        dic[key] = value;
        AnsiConsole.MarkupLine($":left_arrow: [yellow]{value}[/]");
        return value;
    }

}