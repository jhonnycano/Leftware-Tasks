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

    internal async Task Run(string[] args)
    {
        await _databaseInitializer.ValidateAsync();

        var parser = Parser.Default.ParseArguments<InteractiveConsoleOptions, ExecuteTaskOptions>(args);
        await parser.WithParsedAsync<InteractiveConsoleOptions>(StartConsoleMode);
        await parser.WithParsedAsync<ExecuteTaskOptions>(StartTaskExecutionMode);
        await parser.WithNotParsedAsync(ShowInvocationErrors);
        //await parser.WithParsedAsync<WinOptions>(StartWinMode);
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
        var ctx = new TaskExecutionContext();

        try
        {
            await _consoleMode.Execute(ctx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Console mode");
            AnsiConsole.WriteException(ex);
        }
    }

    private async Task StartTaskExecutionMode(ExecuteTaskOptions options)
    {
        try
        {
            var task = options.Task ?? throw new ArgumentException(nameof(options.Task));
            var taskParams = options.TaskParams ?? throw new ArgumentException(nameof(options.TaskParams));
            await _taskExecutor.Execute(task, taskParams.ToArray());

            if (options.Pause)
            {
                UtilConsole.Pause();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Task execution mode");
            AnsiConsole.WriteException(ex);
        }
    }

    private async Task ShowInvocationErrors(IEnumerable<Error> errs)
    {
        foreach (var err in errs)
        {
            Console.WriteLine(err);
        };
        await Task.CompletedTask;
    }
}
