// See https://aka.ms/new-console-template for more information
using Leftware.Tasks.UI;
using Spectre.Console;

var app = Initializer.Initialize();
app.Run(args);
return;


AnsiConsole.Write(new FigletText("LEFTWARE").LeftAligned().Color(Color.Orange1));
AnsiConsole.Write(new FigletText("TASKS").LeftAligned().Color(Color.Orange1));

var choices = new[] {
    "Azure - Clone cosmos container",
    "Azure - Send service bus message to topic",
    "Azure - Receive service bus message in topic",
    "Files - Rename",
    "Azure - Clone cosmos container",
    "Azure - Send service bus message to topic",
    "Azure - Receive service bus message in topic",
    "Files - Rename",
    "Azure - Clone cosmos container",
    "Azure - Send service bus message to topic",
    "Azure - Receive service bus message in topic",
    "Files - Rename",
    "Azure - Clone cosmos container",
    "Azure - Send service bus message to topic",
    "Azure - Receive service bus message in topic",
    "Files - Rename",
};
var result = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
    .Title("Select task")
    .PageSize(5)
    .MoreChoicesText("[grey] Move up and down to reveal more tasks[/]")
    .AddChoices(choices)
    );

Console.WriteLine(result);
//AnsiConsole.Confirm("");
//var b = AnsiConsole.Ask<bool>("Test bool");
// var i = AnsiConsole.Ask<int>("Test int");

AnsiConsole.Clear();

AnsiConsole
    .Status()
    .Start("Executing task...", ctx =>
    {
        ctx.Spinner(Spinner.Known.Star);
        ctx.SpinnerStyle(Style.Parse("green"));
        ctx.Status("Task 1");
        Thread.Sleep(2000);
        AnsiConsole.MarkupLine("Task 1 ... [green]Done[/]");
        ctx.Status("Task 2");
        Thread.Sleep(2000);
        AnsiConsole.MarkupLine("Task 2 ... [green]Done[/]");
        ctx.Status("Task 3");
        Thread.Sleep(2000);
        AnsiConsole.MarkupLine("Task 3 ... [green]Done[/]");
    });


Console.ReadLine();