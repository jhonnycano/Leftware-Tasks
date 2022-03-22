using Leftware.Common;
using Leftware.Injection.Attributes;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Runtime.Loader;

namespace Leftware.Tasks.Core;

[Service]
public class CommonTaskTypeFinder
{
    private readonly ILogger _logger;

    public CommonTaskTypeFinder(
        ILogger logger
        )
    {
        _logger = logger;
    }

    public IList<Type> GetTaskTypes()
    {
        var taskTypeList = new List<Type>();

        var assemblies = AssemblyLoadContext.Default.Assemblies;
        foreach (var assembly in assemblies)
        {
            AddTaskTypesFrom(assembly, taskTypeList);
        }

        return taskTypeList;
    }

    private void AddTaskTypesFrom(Assembly assembly, IList<Type> taskTypeList)
    {
        _logger.LogInformation("Locating types in assembly {0}", assembly.FullName);
        var types = UtilReflection.GetImplementers<CommonTaskBase>(assembly).ToList();
        if (types.Count > 0) taskTypeList.AddRange(types);
    }
}
