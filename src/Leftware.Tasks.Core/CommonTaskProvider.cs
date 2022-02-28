using Leftware.Injection;
using Leftware.Injection.Attributes;

namespace Leftware.Tasks.Core;

public interface ICommonTaskProvider
{
    CommonTaskBase GetTaskByKey(string key, TaskExecutionContext ctx);
}

[InterfaceImplementationDefault]
public class CommonTaskProvider : ICommonTaskProvider
{
    private readonly IServiceLocator _serviceLocator;
    private readonly ICommonTaskLocator _commonTaskLocator;

    public CommonTaskProvider(
        IServiceLocator serviceLocator,
        ICommonTaskLocator commonTaskLocator
        )
    {
        _serviceLocator = serviceLocator;
        _commonTaskLocator = commonTaskLocator;
    }

    public CommonTaskBase GetTaskByKey(string key, TaskExecutionContext ctx)
    {
        var holder = _commonTaskLocator.FindTask(key) ?? throw new InvalidOperationException($"Task not found by key: {key}");        
        var task = GetTask(holder.TaskType);
        task.Context = ctx;
        return task;
    }

    private CommonTaskBase GetTask(Type type)
    {
        var constructorList = type.GetConstructors();
        var constructor = constructorList[0];
        var parameters = constructor.GetParameters();

        var list = new List<object>();
        foreach (var parameter in parameters)
        {
            var service = _serviceLocator.GetService(parameter.ParameterType);
            list.Add(service);
        }
        var task = constructor.Invoke(list.ToArray());
        //var task2 = Activator.CreateInstance(type);
        //var task = Activator.CreateInstance(type, list.ToArray());
        //var xp = Expression.New(type, list.ToArray());
        //var lambda = Expression.Lambda(type, xp);
        return (CommonTaskBase)task;
    }
}
