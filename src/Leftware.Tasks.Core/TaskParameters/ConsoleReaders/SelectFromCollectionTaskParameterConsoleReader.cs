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
        var labelToShow = $"[green]{param.Label}. [/]";
        AnsiConsole.Markup(labelToShow);
        if (param.DefaultKey != null)
            if (AnsiConsole.Confirm($"Use current key ({param.DefaultKey}). ?", true))
            {
                AddAndShow(context, param.Name, param.DefaultKey);
                return;
            }
        if (param.DefaultValue != null)
            if (AnsiConsole.Confirm($"Use current value ({param.DefaultValue}). ?", true))
            {

                AddValueAndShow(context, param.Name, param.DefaultValue);
                return;
            }

        string? input;
        var items = PrepareList(param);
        AnsiConsole.WriteLine();
        input = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .AddChoices(items)
            );
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

        /*
        string value;
        if (param.AllowManualEntry)
        {
            value = _inputHelper.GetStringFromCollection(param.Collection, param.Label, param.DefaultValue);
            if (string.IsNullOrEmpty(value))
            {
                context.IsCanceled = true;
                return;
            }
        }
        else
        {
            var configValue = _collectionProvider.SelectFromCollection(param.Collection, param.Label, param.DefaultValue, param.ExitValue);
            if (configValue == null)
            {
                context.IsCanceled = true;
                return;
            }
            value = param.UseKeyAsValue ? configValue.Name : configValue.Value;
        }

        context[param.Name] = value;
        */
    }

    private string? ReadManual(ConsoleReadContext context, SelectFromCollectionTaskParameter param)
    {
        var header = _collectionProvider.GetHeader(param.Collection) ?? throw new InvalidOperationException($"Collection not found. {param.Collection}");
        string? result = null;
        while (true)
        {
            result = AnsiConsole.Prompt(
                new TextPrompt<string>("[blue] :>[/]")
                .AllowEmpty()
                );

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
