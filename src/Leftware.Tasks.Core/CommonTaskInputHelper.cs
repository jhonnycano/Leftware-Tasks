using Leftware.Common;
using Leftware.Injection.Attributes;
using Leftware.Tasks.Core.Model;
using Spectre.Console;
using System.Text.RegularExpressions;

namespace Leftware.Tasks.Core;

[Service]
public class CommonTaskInputHelper
{
    public ICollectionProvider CollectionProvider { get; }

    public CommonTaskInputHelper(
        ICollectionProvider collectionProvider
        )
    {
        CollectionProvider = collectionProvider;
    }

    public bool GetString(IDictionary<string, object> dic, string key, string label, string? currentValue = null, bool allowEmpty = false)
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

        if (!allowEmpty && string.IsNullOrWhiteSpace(itemValue)) return false;

        AddAndShow(dic, key, itemValue);
        return true;
    }

    public bool GetInt(IDictionary<string, object> dic, string key, string label, int min = 0, int max = 100, int? currentValue = null)
    {
        var labelToShow = $"[green]{label}. [/]";
        AnsiConsole.Markup(labelToShow);
        if (currentValue != null)
        {
            if (AnsiConsole.Confirm($"Use current value ({currentValue}). ?", true))
            {
                AddAndShow(dic, key, currentValue.Value);
                return true;
            }
        }

        int? itemValue = AnsiConsole.Prompt(
            new TextPrompt<int?>($"[blue] [[{min}-{max}]]:>[/]")
            .Validate(n => n >= min && n <= max)
            .AllowEmpty()
            );

        if (itemValue == null) return false;

        AddAndShow(dic, key, itemValue.Value);
        return true;
    }

    public bool GetStringFromCollection(IDictionary<string, object> dic, string key, string label, string collection,
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
        var items = CollectionProvider
            .GetItems(collection)
            .Select(i => i.Label)
            .Append(Defs.MANUAL_INPUT_LABEL)
            .Append(Defs.CANCEL_LABEL)
            .ToList();
        AnsiConsole.WriteLine();
        itemValue = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .AddChoices(items)
            );
        if (itemValue == Defs.CANCEL_LABEL) return false;
        if (itemValue == Defs.MANUAL_INPUT_LABEL)
        {
            itemValue = AnsiConsole.Prompt(
                new TextPrompt<string>("[blue] :>[/]")
                );

            if (string.IsNullOrWhiteSpace(itemValue)) return false;
        }

        AddAndShow(dic, key, itemValue);
        return true;
    }

    public bool GetStringValidRegex(IDictionary<string, object> dic, string key, string label,
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

    public bool GetExistingFile(IDictionary<string, object> dic, string key, string label,
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

    public bool GetJson(IDictionary<string, object> dic, string key, string label,
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

    public CollectionItem? GetItem(IDictionary<string, object> dic, string key, string label, string collection)
    {
        var items = CollectionProvider.GetItems(collection);
        if (items.Count == 0)
        {
            AnsiConsole.MarkupLine($"[yellow]Empty collection[/] [magenta]{collection}[/]. Please add first some items to the collection before executing this task");
            return null;
        }

        var itemLabels = items.Select(i => i.Label).Append(Defs.CANCEL_LABEL);

        var labelToShow = $"[green]{label}. [/]";
        AnsiConsole.MarkupLine(labelToShow);

        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .AddChoices(itemLabels)
            );
        var result = items.FirstOrDefault(i => i.Label == selection);
        if (result == null) return null;

        AddAndShow(dic, key, result.Key);
        return result;
    }

    public string? SelectOption(IDictionary<string, object> dic, string key, string label, IEnumerable<string> options)
    {
        var labelToShow = $"[green]{label}. [/]";
        AnsiConsole.Markup(labelToShow);
        var newOptions = options.Append(Defs.CANCEL_LABEL);
        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .AddChoices(newOptions)
            );
        if (selection == null || selection == Defs.CANCEL_LABEL) return null;

        AddAndShow(dic, key, selection);
        return selection;
    }

    internal bool IsValidRegexOrEmpty(string arg, string regex)
    {
        if (string.IsNullOrWhiteSpace(arg)) return true;
        return Regex.IsMatch(arg, regex);
    }

    internal bool IsExistingFileOrEmpty(string arg)
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

    internal bool IsValidJsonForSchema(string json, string schemaJson)
    {
        return UtilJsonSchema.IsValidJsonForSchema(json, schemaJson);
    }

    internal T AddAndShow<T>(IDictionary<string, object> dic, string key, T value)
    {
        dic[key] = value;
        AnsiConsole.MarkupLine($":left_arrow: [yellow]{value}[/]");
        return value;
    }

    public bool IsValidFileInput(string input, IShouldExist shouldExist)
    {
        input = input.Trim();
        var dir = Path.GetDirectoryName(input);
        if (string.IsNullOrEmpty(dir))
        {
            dir = Environment.CurrentDirectory;
            input = Path.Combine(dir, input);
        }
        if (!Directory.Exists(dir))
        {
            UtilConsole.WriteError("File path not found");
            return false;
        }
        if (shouldExist.ShouldExist && !File.Exists(input))
        {
            UtilConsole.WriteError("File not found");
            return false;
        }
        return true;
    }

    public bool IsValidFolderInput(string input, IShouldExist shouldExist)
    {
        input = input.Trim();
        var dir = Path.GetDirectoryName(input);
        if (string.IsNullOrEmpty(dir))
        {
            dir = Environment.CurrentDirectory;
            input = Path.Combine(dir, input);
        }
        if (!Directory.Exists(dir))
        {
            UtilConsole.WriteError("Folder path not found");
            return false;
        }
        if (shouldExist.ShouldExist && !Directory.Exists(input))
        {
            UtilConsole.WriteError("Folder not found");
            return false;
        }
        return true;
    }

    public bool IsValidSchemaInput(string input, string? schema)
    {
        return UtilJsonSchema.IsValidJsonForSchema(input, schema);
    }


}
