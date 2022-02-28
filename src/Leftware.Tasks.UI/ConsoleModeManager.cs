using Leftware.Common;
using Leftware.Injection.Attributes;
using Leftware.Tasks.Resources;
using Leftware.Tasks.UI.Model;
using Spectre.Console;
using System.Text.RegularExpressions;

namespace Leftware.Tasks.Core;

[Service]
public class ConsoleModeManager
{
    private readonly ICommonTaskLocator _commonTaskLocator;
    private readonly ICommonTaskProvider _commonTaskProvider;

    //private readonly IMacroManager _macroManager;
    //private readonly ICollectionProvider _collectionProvider;
    private IList<CommonTaskHolder> _holderList;
    private CommonTaskHolder? _lastHolder;
    private IDictionary<string, object>? _lastData;

    public ConsoleModeManager(
        ICommonTaskLocator consoleTaskLocator,
        ICommonTaskProvider commonTaskProvider
        //IMacroManager macroManager,
        //ICollectionProvider collectionProvider
        )
    {
        _commonTaskLocator = consoleTaskLocator;
        _commonTaskProvider = commonTaskProvider;
        //_macroManager = macroManager;
        //_collectionProvider = collectionProvider;
        _holderList = _commonTaskLocator.GetTaskHolderList();
    }

    public async Task Execute(TaskExecutionContext ctx)
    {
        SetupLanguage();
        ctx.HolderList = _holderList;

        var currentList = _holderList;
        var pageInfo = SetupPageInfo(currentList);
        _lastHolder = null;
        _lastData = null;

        while (true)
        {
            UtilConsole.Clear();
            ShowWelcomeMessage();
            ShowTasks(currentList, pageInfo);
            Console.Write("Select task :>");
            var opt = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(opt))
            {
                currentList = _holderList;
                pageInfo = SetupPageInfo(currentList);
                continue;
            }

            CommonTaskHolder? selectedHolder = null;

            bool askForData = true;
            if (opt == "!")
            {
                if (_lastHolder != null) selectedHolder = _lastHolder;
            }
            else if (opt == "!!")
            {
                if (_lastHolder != null) selectedHolder = _lastHolder;
                askForData = false;
            }
            else if (Regex.IsMatch(opt, "^!\\d+$"))
            {
                /*
                var favoriteMacroKey = opt.Substring(1);
                var key = int.Parse(favoriteMacroKey);
                var macro = _macroManager.GetFavoriteMacro(key);
                if (!string.IsNullOrEmpty(macro))
                {
                    _macroManager.ExecuteMacroFromFile(macro, ctx);
                    UtilConsole.Pause();
                    continue;
                }
                */
            }
            else if (opt.ToUpperInvariant() == "N")
            {
                pageInfo.CurrentPage++;
                if (pageInfo.CurrentPage > pageInfo.TotalPages)
                    pageInfo.CurrentPage = pageInfo.TotalPages;
                continue;
            }
            else if (opt.ToUpperInvariant() == "P")
            {
                pageInfo.CurrentPage--;
                if (pageInfo.CurrentPage < 1)
                    pageInfo.CurrentPage = 1;
                continue;
            }

            if (selectedHolder == null)
            {
                var filteredTasks = FilterTasks(currentList, opt);
                if (filteredTasks == null)
                {
                    currentList = _holderList;
                    pageInfo = SetupPageInfo(currentList);
                    continue;
                }
                if (filteredTasks.Count > 1)
                {
                    currentList = filteredTasks;
                    pageInfo = SetupPageInfo(currentList);
                    continue;
                }
                if (filteredTasks.Count == 0) continue;

                selectedHolder = filteredTasks[0];
            }

            try
            {
                UtilConsole.Clear();
                var description = GetTaskDescription(selectedHolder);
                Console.WriteLine("Task: {0}", selectedHolder.Name);
                Console.WriteLine();

                if (!string.IsNullOrEmpty(description))
                {
                    Console.WriteLine(description);
                    Console.WriteLine();
                }

                _lastHolder = selectedHolder;
                var taskInstance = _commonTaskProvider.GetTaskByKey(selectedHolder.Key, ctx);

                // ask for input
                if (askForData)
                {
                    var dic = await taskInstance.GetTaskInput();
                    if (dic == null)
                    {
                        Console.WriteLine("No suitable input to execute data. Press enter to continue");
                        UtilConsole.Pause();
                        continue;
                    }
                    _lastData = dic;
                }

                if (!ctx.TasksToSkipInMacroRecord.Contains(selectedHolder.Key))
                {
                    /*
                    if (ctx.MacroHolder != null)
                    {
                        var taskKey = selectedTask.Key;
                        var macroTask = new MacroTask { Task = taskKey, Parameters = _lastData };
                        ctx.MacroHolder.Tasks.Add(macroTask);
                    }
                    */
                }

                var task = LogTaskInvocation(selectedHolder, _lastData);

                // execute
                if (!ctx.SkipExecution)
                {
                    if (_lastData == null)
                    {
                        Console.WriteLine("Previous data not found for replaying task");
                    }
                    else
                    {
                        await taskInstance.Execute(_lastData);
                    }
                }

                task?.Wait();
            }
            catch (UserExitException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error: {0}, {1}", ex.Message, ex.StackTrace);
            }

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Task '{0}' completed", selectedHolder.Name);
            Console.ResetColor();
            UtilConsole.Pause();
        }
    }

    private void SetupLanguage()
    {
        /*
        var language = _collectionProvider.GetSetting(Definitions.SettingLanguage);
        if (string.IsNullOrEmpty(language))
        {
            _collectionProvider.SetSetting(Definitions.SettingLanguage, "en");
            language = "en";
        }
        CultureInfo cultureInfo = new CultureInfo(language);
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;
        */
    }

    private static IList<CommonTaskHolder> FilterTasks(IList<CommonTaskHolder> currentTasks, string option)
    {
        int selectedIndex;
        var isInteger = int.TryParse(option, out selectedIndex);
        if (isInteger)
        {
            if (selectedIndex > 0 && selectedIndex <= currentTasks.Count)
                return new List<CommonTaskHolder> { currentTasks[selectedIndex - 1] };
        }
        var result = currentTasks
            .Where(t => t.Name.IndexOf(option, StringComparison.InvariantCultureIgnoreCase) != -1)
            .OrderBy(t => t.Name)
            .ToList();

        return result;
    }

    private static void ShowTasks(IList<CommonTaskHolder> currentTasks, PageInfo pageInfo)
    {
        var lower = (pageInfo.CurrentPage - 1) * pageInfo.PageSize;
        var upper = pageInfo.CurrentPage * pageInfo.PageSize;
        if (upper > currentTasks.Count) upper = currentTasks.Count;

        for (int index = lower; index < upper; index++)
        {
            var currentTask = currentTasks[index];
            var name = currentTask.Name;
            AnsiConsole.MarkupLine("[yellow]{0,6}[/]  {1}", index + 1, name);
        }
        AnsiConsole.MarkupLine("Page [yellow]{0}[/] of [yellow]{1}[/]. Press [underline yellow]N[/] for Next Page, or [underline yellow]P[/] for Previous Page",
            pageInfo.CurrentPage, pageInfo.TotalPages);
    }

    private static Task<string> LogTaskInvocation(CommonTaskHolder holder, IDictionary<string, object> lastData)
    {
        return null;
    }

    private static string GetTaskDescription(CommonTaskHolder holder)
    {
        // todo: buscar descripción de tarea en recursos o en bd a partir del tipo o nombre de la tarea
        return null; // task.Description;
    }

    private static void ShowWelcomeMessage()
    {
        Console.WriteLine(StringResources.Console_Welcome);
    }

    private static PageInfo SetupPageInfo(IList<CommonTaskHolder> tasks)
    {
        var result = new PageInfo
        {
            PageSize = 12
        };
        result.TotalPages = tasks.Count / result.PageSize + 1;
        result.CurrentPage = 1;
        return result;
    }
}
