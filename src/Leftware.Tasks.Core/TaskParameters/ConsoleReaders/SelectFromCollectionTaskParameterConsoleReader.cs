using Spectre.Console;

namespace Leftware.Tasks.Core.TaskParameters.ConsoleReaders;

internal class SelectFromCollectionTaskParameterConsoleReader : TaskParameterConsoleReaderBase<SelectFromCollectionTaskParameter>
{
    private readonly ICollectionProvider _collectionProvider;
    private readonly CommonTaskInputHelper _inputHelper;

    public SelectFromCollectionTaskParameterConsoleReader(
        ICollectionProvider collectionProvider,
        CommonTaskInputHelper inputHelper
        )
    {
        _collectionProvider = collectionProvider;
        _inputHelper = inputHelper;
    }
    public override void Read(ConsoleReadContext context, SelectFromCollectionTaskParameter param)
    {
        if (AskIfDefaultValue(context, param)) return;
        if (AskIfDefaultKey(context, param)) return;

        var labelForPrompt = GetLabelForPrompt(param);
        AnsiConsole.Markup(labelForPrompt);

        string? input;
        var items = PrepareList(param);
        AnsiConsole.WriteLine();

        var prompt = new SelectionPrompt<string>()
            .AddChoices(items);
        input = AnsiConsole.Prompt(prompt);
        if (input == Defs.CANCEL_LABEL)
        {
            context.IsCanceled = true;
            return;
        }
        else if (input == Defs.MANUAL_INPUT_LABEL)
        {
            input = ReadManual(context, param);
            if (input == null) return;
        }

        AddValueAndShow(context, param.Name, input);
    }

    public bool AskIfDefaultKey(ConsoleReadContext context, SelectFromCollectionTaskParameter param)
    {
        if (param.DefaultValue == null) return false;

        var defaultLabel = $"Use default key ({param.DefaultKey})?";
        var label = $"[green]{param.Label}[/]. {defaultLabel}";
        if (AnsiConsole.Confirm(label, true))
        {
            AddAndShow(context, param.Name, param.DefaultKey);
            return true;
        }
        return false;
    }

    private string? ReadManual(ConsoleReadContext context, SelectFromCollectionTaskParameter param)
    {
        var header = _collectionProvider.GetHeader(param.Collection) ?? throw new InvalidOperationException($"Collection not found. {param.Collection}");
        while (true)
        {
            var prompt = new TextPrompt<string>("[blue] :>[/]")
                            .AllowEmpty();
            var result = AnsiConsole.Prompt(prompt);

            if (string.IsNullOrWhiteSpace(result)) continue;
            
            if (result == param.CancelString)
            {
                context.IsCanceled = true;
                return null;
            }

            switch (header.ItemType)
            {
                case CollectionItemType.String:
                    return result;
                case CollectionItemType.File:
                    if (!_inputHelper.IsValidFileInput(result, param)) continue;
                    return result;
                case CollectionItemType.Folder:
                    if (!_inputHelper.IsValidFolderInput(result, param)) continue;
                    return result;
                case CollectionItemType.JsonObject:
                    if (!_inputHelper.IsValidSchemaInput(result, header.Schema)) continue;
                    return result;
            }
        }
    }

    private IList<string> PrepareList(SelectFromCollectionTaskParameter param)
    {
        var list = _collectionProvider
            .GetItems(param.Collection)
            .Select(i => i.Label);
        if (param.AllowManualEntry) list = list.Append(Defs.MANUAL_INPUT_LABEL);
        list = list.Append(Defs.CANCEL_LABEL);
        return list.ToList();
    }

    private void AddValueAndShow(ConsoleReadContext context, string key, string value)
    {
        context[key] = Defs.USE_AS_VALUE;
        context[$"name__$rawValue"] = value;
        AnsiConsole.MarkupLine($":left_arrow: [blue]{key}: [/] [yellow]{value}[/]");
    }
}
