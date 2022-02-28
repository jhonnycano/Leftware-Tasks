using Spectre.Console;

namespace Leftware.Tasks.Core;

public abstract class CommonTaskBase
{
    public TaskExecutionContext? Context { get; set; }

    public async virtual Task<IDictionary<string, object>?> GetTaskInput()
    {
        return await Task.FromResult(default(IDictionary<string, object>));
    }

    public abstract Task Execute(IDictionary<string, object> input);

    protected static IDictionary<string, object> GetEmptyTaskInput()
    {
        return new Dictionary<string, object>();
    }

    protected bool GetString(
        IDictionary<string, object> dic, 
        string key, 
        string label,
        string? currentValue = null, 
        string? sourceCollection = null)
    {
        var labelToShow = $"[green]{label}[/]";
        AnsiConsole.MarkupLine(labelToShow);
        if (currentValue != null)
        {
            if (AnsiConsole.Confirm($"Current value is \"{currentValue}\". Use current?", true))
            {
                dic[key] = currentValue;
                return true;
            }
        }

        string itemValue;
        if (sourceCollection != null)
        {
            var items = Context!.CollectionProvider
                .GetItems(sourceCollection)
                .Select(i => i.Label)
                .Append("-- Input manual value")
                .ToList();
            
            itemValue = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .AddChoices(items)
                );
        }
        else
        {
            itemValue = "-- Input manual value";
        }

        if (itemValue == "-- Input manual value")
        {
            itemValue = AnsiConsole.Prompt(
                new TextPrompt<string>("[blue]Manual input :>[/]")
                );

            if (string.IsNullOrWhiteSpace(itemValue)) return false;
        }

        dic[key] = itemValue;
        return true;
    }

}