using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.TaskParameters;

namespace Leftware.Tasks.Impl.General.WebServers.IIS;

[Descriptor("IIS - Create IIS pool")]
internal class CreateIISPoolTask : CommonTaskBase
{
    private const string POOL = "pool";
    private const string FRAMEWORK = "framework";

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        return new List<TaskParameter>
        {
            new ReadStringTaskParameter(POOL, "Pool name"),
            new SelectStringTaskParameter(FRAMEWORK, "Framework", new[] {"1.0", "1.1", "2.0", "4.0"}),
        };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        var name = input.Get<string>(POOL);
        var framework = input.Get<string>(FRAMEWORK);

        var pathAppcmd = Context.SettingsProvider.GetSetting(Defs.Settings.PATH_APPCMD, true);
        if (string.IsNullOrEmpty(pathAppcmd))
        {
            Console.WriteLine("path for executable Appcmd not found. Operation cancelled");
            return;
        }

        var runtime = GetRuntime(framework);
        var pipe = GetPipemode();
        var template =
            "add apppool \"/name:{{name}}\" \"/managedRuntimeVersion:{{runtime}}\" \"/managedPipelineMode:{{pipe}}\"";
        var cmd = template.FormatLiquid(new { name, runtime, pipe });

        var result = UtilProcess.Invoke(pathAppcmd, cmd);
        Console.WriteLine(result);
    }

    private static string GetRuntime(string framework)
    {
        if (framework == "1.0") return "v1.0";
        if (framework == "1.1") return "v1.1";
        if (framework == "2.0") return "v2.0";
        return "v4.0";
    }

    private static string GetPipemode()
    {
        return "Integrated";
    }
}
