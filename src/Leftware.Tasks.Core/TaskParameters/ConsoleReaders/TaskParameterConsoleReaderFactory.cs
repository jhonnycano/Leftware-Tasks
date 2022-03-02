using Leftware.Common;
using Leftware.Injection;
using Leftware.Injection.Attributes;

namespace Leftware.Tasks.Core.TaskParameters.ConsoleReaders;

public interface ITaskParameterConsoleReaderFactory : IFactory<TaskParameterType, TaskParameterConsoleReaderBase>
{
}

[InterfaceImplementationDefault]
public class TaskParameterConsoleReaderFactory : FactoryBase<TaskParameterType, TaskParameterConsoleReaderBase>,
    ITaskParameterConsoleReaderFactory
{
    public TaskParameterConsoleReaderFactory(IServiceLocator serviceLocator) : base(serviceLocator)
    {
        Types = new Dictionary<TaskParameterType, Type>
            {
                {TaskParameterType.ReadBool, typeof(ReadBoolTaskParameterConsoleReader)},
                {TaskParameterType.ReadInteger, typeof(ReadIntegerTaskParameterConsoleReader)},
                {TaskParameterType.ReadString, typeof(ReadStringTaskParameterConsoleReader)},
                {TaskParameterType.ReadPassword, typeof(ReadPasswordTaskParameterConsoleReader)},
                {TaskParameterType.SelectString, typeof(SelectStringTaskParameterConsoleReader)},
                {TaskParameterType.SelectFromCollection, typeof(SelectFromCollectionTaskParameterConsoleReader)},
                {TaskParameterType.SelectEnum, typeof(SelectEnumTaskParameterConsoleReader)},
                {TaskParameterType.ReadFolder, typeof(ReadFolderTaskParameterConsoleReader)},
                {TaskParameterType.ReadFile, typeof(ReadFileTaskParameterConsoleReader)},
                {TaskParameterType.SelectGroup, typeof(SelectGroupTaskParameterConsoleReader)}
            };
    }

    protected override IDictionary<TaskParameterType, Type> Types { get; }
}
