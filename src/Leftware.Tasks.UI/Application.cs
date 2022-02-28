using CommandLine;
using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Persistence;
using Leftware.Tasks.UI.Verbs;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace Leftware.Tasks.UI;

internal class Application
{
    private readonly ConsoleModeManager _consoleMode;
    private readonly SqliteDatabaseProvider _provider;
    private readonly CollectionInitializer _collectionInitializer;
    private readonly ILogger _logger;

    /*
private readonly ConsoleTaskExecutor _consoleTaskExecutor;
*/

    public Application(
        ConsoleModeManager consoleMode,
        SqliteDatabaseProvider provider,
        CollectionInitializer collectionInitializer,
        ILogger logger
        /*
        ConsoleTaskExecutor consoleTaskExecutor,
        */
        )
    {
        _consoleMode = consoleMode;
        _provider = provider;
        _collectionInitializer = collectionInitializer;
        _logger = logger;
        /*
_consoleTaskExecutor = consoleTaskExecutor;
*/
    }

    internal void Run(string[] args)
    {
        //_databaseProvider.GetOpenConnection(true);

        Parser.Default
            .ParseArguments<InteractiveConsoleOptions, ExecuteTaskOptions>(args)
            //.WithParsed<WinOptions>(opts => StartWinMode(opts))
            .WithParsed<InteractiveConsoleOptions>(opts => StartConsoleMode(opts))
            .WithParsed<ExecuteTaskOptions>(opts => StartTaskExecutionMode(opts))
            .WithNotParsed(errs => ShowInvocationErrors(errs));
    }

    /*
    private void StartWinMode(WinOptions options)
    {
        System.Windows.Forms.Application.SetHighDpiMode(HighDpiMode.SystemAware);
        System.Windows.Forms.Application.EnableVisualStyles();
        System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

        System.Windows.Forms.Application.Run(new FrmMain());
    }

    */

    private async Task StartConsoleMode(InteractiveConsoleOptions options)
    {
        //_consoleTaskInitializer.Initialize();

        var ctx = new TaskExecutionContext
        {
            /*
            TasksToSkipInMacroRecord = new List<string> {
                    "Leftware.Utils.UI.Tasks.Macros.StopRecordMacroConsoleTask",
                    "Leftware.Utils.UI.Tasks.General.UserExitConsoleTask"
                }
            */
        };

        try
        {
            await _collectionInitializer.ValidateAsync();
            await _consoleMode.Execute(ctx);
        }
        catch (Exception ex)
        {
            var msg = $"General exception: {ex.Message}";
            _logger.LogError(ex, msg);
            AnsiConsole.MarkupLine($"[olive]{msg}[/]");
        }
    }

    private void StartTaskExecutionMode(ExecuteTaskOptions options)
    {
        //_consoleTaskExecutor.Execute(options.Task, options.TaskParams.ToArray());

        if (options.Pause)
        {
            UtilConsole.Pause();
        }
    }

    private void ShowInvocationErrors(IEnumerable<Error> errs)
    {
        foreach (var err in errs)
        {
            Console.WriteLine(err);
        };
    }
}
