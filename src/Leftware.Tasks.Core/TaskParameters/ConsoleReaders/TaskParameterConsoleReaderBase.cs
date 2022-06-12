using Newtonsoft.Json.Linq;
using Spectre.Console;

namespace Leftware.Tasks.Core.TaskParameters.ConsoleReaders;

public abstract class TaskParameterConsoleReaderBase
{
    public abstract void Read(ConsoleReadContext context, TaskParameter param);
}

public abstract class TaskParameterConsoleReaderBase<T> : TaskParameterConsoleReaderBase where T : TaskParameter
{
    public override void Read(ConsoleReadContext context, TaskParameter param)
    {
        Read(context, (T)param);
    }

    public abstract void Read(ConsoleReadContext context, T param);

    protected T AddAndShow<T>(ConsoleReadContext ctx, string key, T value)
    {
        ctx[key] = value;
        AnsiConsole.Markup($"* [blue]{key}: [/]");
        Console.WriteLine(value);
        return value;
    }

    public bool AskIfDefaultValue(ConsoleReadContext context, IHasDefault param)
    {
        if (param.DefaultValue == null) return false;

        var defaultValueToUse = GetDefaultValueToUse(context, param.DefaultValue?.ToString() ?? "");
        var defaultValueToShow = param.DefaultValueLabel ?? defaultValueToUse;

        var defaultLabel = $"Use default value ({defaultValueToShow})?";
        var label = $"[green]{param.Label}[/]. {defaultLabel}";
        if (AnsiConsole.Confirm(label, true))
        {
            AddAndShow(context, param.Name, defaultValueToUse);
            return true;
        }
        return false;
    }

    private string GetDefaultValueToUse(ConsoleReadContext context, string value)
    {
        if (!value.StartsWith("->")) return value;

        value = value[2..];
        var indexOfPipe = value.IndexOf("|");
        if (indexOfPipe == -1)
        {
            value = context.Values[value].ToString();
            return value;
        }

        var prop = value.Split('|');
        var colName = prop[0];
        var itemKey = prop[1];
        var query = prop[2];

        var itemName = context.Values[itemKey].ToString();
        var item = context.CollectionProvider.GetItem(colName, itemName);
        var json = JObject.Parse(item.Content);
        var result = json.SelectToken(query);
        value = result.ToString();
        return value;
    }

    protected static string GetLabelForPrompt(TaskParameter param)
    {
        var formattedLabel = $"[green]{param.Label}. [/]";
        var promptString = $"[blue] :>[/]";
        var labelForPrompt = $"{formattedLabel}{promptString}";
        return labelForPrompt;
    }

    protected bool CancellableValidator(string s, TaskParameter param, Func<string, bool> validator)
    {
        return s == param.CancelString || validator(s);
    }

    protected bool CancellableValidator(string s, TaskParameter param, ConsoleReadContext ctx, Func<string, ConsoleReadContext, bool> validator)
    {
        return s == param.CancelString || validator(s, ctx);
    }

}
