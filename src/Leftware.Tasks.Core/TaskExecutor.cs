using Leftware.Common;
using Leftware.Injection.Attributes;
using Spectre.Console;

namespace Leftware.Tasks.Core;

[Service]
public class TaskExecutor
{
    private readonly ICommonTaskLocator _taskLocator;
    private readonly ICommonTaskProvider _taskProvider;
    private readonly ICollectionProvider _collectionProvider;
    private readonly ISettingsProvider _settingsProvider;

    public TaskExecutor(
        ICommonTaskLocator taskLocator, 
        ICommonTaskProvider taskProvider,
        ICollectionProvider collectionProvider,
        ISettingsProvider settingsProvider
        )
    {
        _taskLocator = taskLocator;
        _taskProvider = taskProvider;
        _collectionProvider = collectionProvider;
        _settingsProvider = settingsProvider;
    }

    public async Task Execute(string taskKey, string[] parameters)
    {
        var (taskHolder, error) = GetTaskHolder(taskKey);
        if (taskHolder == null)
        {
            Console.WriteLine(error);
            return;
        }

        var dic = UtilCollection.ExtractParams(parameters).ToDictionary(itm => itm.Key, itm => (object)itm.Value);

        var ctx = new TaskExecutionContext
        {
            CollectionProvider = _collectionProvider,
            SettingsProvider = _settingsProvider
        };
        var taskInstance = _taskProvider.GetTaskByKey(taskHolder.Key, ctx);

        await AnsiConsole
            .Status()
            .Start($"Executing task [green]{taskHolder.Name}[/]...", async statusContext => {
                ctx.StatusContext = statusContext;
                statusContext
                    .Spinner(Spinner.Known.Star)
                    .SpinnerStyle(Style.Parse("green"));
                AnsiConsole.WriteLine();
                AnsiConsole.Write(new Rule());
                AnsiConsole.WriteLine();
                await taskInstance.Execute(dic);
            });
    }

    private (CommonTaskHolder? taskHolder, string? error) GetTaskHolder(string taskKey)
    {
        var taskHolderList = _taskLocator.GetTaskHolderList();
        var holderByExactFullName = taskHolderList.FirstOrDefault(h => string.Equals(h.Key, taskKey, StringComparison.InvariantCultureIgnoreCase));
        if (holderByExactFullName != null) return (holderByExactFullName, null);
        var holderListByEnd = taskHolderList.Where(h => h.Key.Contains(taskKey, StringComparison.InvariantCultureIgnoreCase)).ToList();
        if (holderListByEnd.Count == 1) 
            return (holderListByEnd.First(), null);
        else if (holderListByEnd.Count > 1)
        {
            return (null, "Pleaase specify the exact task to execute. Tasks found: " + string.Join(", ", holderListByEnd));
        }

        return (null, "Task not found");
    }
}
