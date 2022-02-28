using Leftware.Common;
using Leftware.Injection;

namespace Leftware.Tasks.Core;

public class CommonTaskInjector
{
    private readonly ICommonTaskLocator _commonTaskLocator;

    public CommonTaskInjector(ICommonTaskLocator commonTaskLocator)
    {
        _commonTaskLocator = commonTaskLocator;
    }

    public void Inject(IInjectionProvider injectionProvider)
    {
        IList<CommonTaskHolder>? taskList = _commonTaskLocator.GetTaskHolderList();
        foreach (var task in taskList)
        {
            injectionProvider.Add(task.TaskType, task.TaskType, LifetimeMode.Transient);
        }
    }
}
