using Leftware.Common;
using Leftware.Injection.Attributes;
using System.Runtime.Loader;

namespace Leftware.Tasks.Core;

public interface ICommonTaskLocator
{
    IList<CommonTaskHolder> GetTaskHolderList();

    CommonTaskHolder? FindTask(string key);
}

[InterfaceImplementationDefault]
public class CommonTaskLocator : ICommonTaskLocator
{
    private readonly CommonTaskTypeFinder _commonTaskTypeLocator;
    private IList<CommonTaskHolder> _taskHolderList;

    public CommonTaskLocator(CommonTaskTypeFinder commonTaskTypeLocator)
    {
        _commonTaskTypeLocator = commonTaskTypeLocator;
        _taskHolderList = new List<CommonTaskHolder>();
    }

    public IList<CommonTaskHolder> GetTaskHolderList()
    {
        if (_taskHolderList.Count > 0) return _taskHolderList;

        var taskTypeList = _commonTaskTypeLocator.GetTaskTypes();
        foreach (var taskType in taskTypeList)
        {
            var holder = CreateHolder(taskType);
            _taskHolderList.Add(holder);
        }

        _taskHolderList = _taskHolderList
            .OrderBy(th => th.SortOrder)
            .ThenBy(th => th.Name)
            .ToList();

        return _taskHolderList;
    }

    public CommonTaskHolder? FindTask(string key)
    {
        var assemblies = AssemblyLoadContext.Default.Assemblies;
        foreach (var assembly in assemblies)
        {
            foreach (var type in UtilReflection.GetImplementers<CommonTaskBase>(assembly))
            {
                if (key != type.FullName) continue;

                CommonTaskHolder holder = CreateHolder(type);
                return holder;
            }
        }

        return null;
    }

    private static CommonTaskHolder CreateHolder(Type taskType)
    {
        if (taskType.FullName == null) throw new InvalidOperationException($"Found task without type name. Type: {taskType.GUID}");

        var descriptor = UtilReflection.GetAttribute<DescriptorAttribute>(taskType) ??
           throw new InvalidOperationException($"Found task without descriptor. Type: {taskType.FullName}");

        return new CommonTaskHolder
            (
            taskType.FullName,
            descriptor.Name,
            descriptor.SortOrder,
            descriptor.ConfirmBeforeExecution,
            taskType
           );
    }
}
