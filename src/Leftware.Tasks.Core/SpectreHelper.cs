using Spectre.Console;

namespace Leftware.Tasks.Core;

public static class SpectreHelper
{
    public static void WriteTaskHeader(string taskName)
    {
        Console.WriteLine();
        var table = new Table();
        table.AddColumn(new TableColumn("").Centered());
        table.HideHeaders();
        table.Width(70);
        table.AddRow("[gold1]Executing task[/]").Centered();
        table.AddRow($"[steelblue1]{taskName}[/]").Centered();
        AnsiConsole.Write(table);
        Console.WriteLine();
    }
}
