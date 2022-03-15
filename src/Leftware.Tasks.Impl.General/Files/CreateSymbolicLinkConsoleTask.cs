using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.TaskParameters;

namespace Leftware.Tasks.Impl.General.Files;

[Descriptor("FileSystem - Create symbolic link")]
internal class CreateSymbolicLinkConsoleTask : CommonTaskBase
{
    private readonly ICollectionProvider _collectionProvider;
    private const string SOURCE = "source";
    private const string TARGET = "target";

    public CreateSymbolicLinkConsoleTask(ICollectionProvider collectionProvider)
    {
        _collectionProvider = collectionProvider;
    }

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        return new List<TaskParameter>
        {
            new SelectFromCollectionTaskParameter(SOURCE, "Source folder", Defs.Collections.SYMLINK_SOURCE),
            new SelectFromCollectionTaskParameter(TARGET, "Target folder", Defs.Collections.SYMLINK_TARGET),
        };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        var source = input.Get<string>(SOURCE);
        var target = input.Get<string>(TARGET);

        var sourceValue = _collectionProvider.GetItemContentAs<string>(Defs.Collections.SYMLINK_SOURCE, source);
        var targetValue = _collectionProvider.GetItemContentAs<string>(Defs.Collections.SYMLINK_TARGET, target);

        var parameters = @"/c mklink /j ""{targetValue}"" ""{sourceValue}"""
            .FormatLiquid(new { sourceValue, targetValue });

        /*
        var invoker = new CommandLineInvoker("cmd.exe", parameters)
        {
            InvokeMode = CommandLineInvokerMode.UseNoShell
        };
        invoker.Start();
        */
    }
}
