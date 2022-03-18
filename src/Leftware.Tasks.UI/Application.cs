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
    private readonly DatabaseInitializer _databaseInitializer;
    private readonly ILogger _logger;
    private readonly TaskExecutor _taskExecutor;

    public Application(
        ConsoleModeManager consoleMode,
        DatabaseInitializer databaseInitializer,
        TaskExecutor taskExecutor,
        ILogger logger
        )
    {
        _consoleMode = consoleMode;
        _databaseInitializer = databaseInitializer;
        _taskExecutor = taskExecutor;
        _logger = logger;
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
            await _databaseInitializer.ValidateAsync();
            await _consoleMode.Execute(ctx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            AnsiConsole.WriteException(ex);
        }
    }

    private async Task StartTaskExecutionMode(ExecuteTaskOptions options)
    {
        try
        {
            var task = options.Task ?? throw new ArgumentNullException(nameof(options));
            var taskParams = options.TaskParams ?? throw new ArgumentNullException(nameof(options.TaskParams));
            await _taskExecutor.Execute(task, taskParams.ToArray());

            if (options.Pause)
            {
                UtilConsole.Pause();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            AnsiConsole.WriteException(ex);
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
