using Leftware.Injection.Attributes;
using Leftware.Tasks.Core.TaskParameters;
using Leftware.Tasks.Core.TaskParameters.ConsoleReaders;

namespace Leftware.Tasks.Core;

public interface ICommonTaskInputManager
{
    IDictionary<string, object> GetTaskInput(IList<TaskParameter> dicParams);
}

[InterfaceImplementationDefault]
public class CommonTaskInputManager : ICommonTaskInputManager
{
    private readonly ICollectionProvider _collectionProvider;
    private readonly ITaskParameterConsoleReaderFactory _consoleTaskParameterFactory;

    public CommonTaskInputManager(
        ICollectionProvider collectionProvider,
        ITaskParameterConsoleReaderFactory consoleTaskParameterFactory
    )
    {
        _collectionProvider = collectionProvider;
        _consoleTaskParameterFactory = consoleTaskParameterFactory;
    }

    public IDictionary<string, object> GetTaskInput(IList<TaskParameter> dicParams)
    {
        if (dicParams == null) return new Dictionary<string, object>();

        var context = new ConsoleReadContext();
        context.CollectionProvider = _collectionProvider;
        var group = new SelectGroupTaskParameter(dicParams);
        var groupReader = _consoleTaskParameterFactory.GetInstance(group.Type);
        groupReader.Read(context, group);

        if (context.IsCanceled) return null;

        /*
        foreach (var param in dicParams)
        {
            if (!param.Condition(context)) continue;

            var reader = _consoleTaskParameterFactory.GetInstance(param.Type);
            reader.Read(context, param);
            if (context.IsCanceled) return null;
        }
        */
        return context.Values;
    }
}
