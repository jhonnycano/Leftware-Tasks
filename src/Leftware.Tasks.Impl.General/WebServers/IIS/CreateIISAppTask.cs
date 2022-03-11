using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.TaskParameters;

namespace Leftware.Tasks.Impl.General.WebServers.IIS;

[Descriptor("IIS - Create IIS app")]
internal class CreateIISAppTask : CommonTaskBase
{
    private const string SITE = "site";
    private const string NAME = "name";
    private const string PATH = "path";
    private const string POOL = "pool";

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        return new List<TaskParameter>
        {
            new ReadStringTaskParameter(SITE, "Host site").WithDefaultValue("Default Web Site"),
            new ReadStringTaskParameter(NAME, "App name"),
            new ReadFolderTaskParameter(PATH, "Physical path"),
            new ReadStringTaskParameter(POOL, "Pool. Leave empty if no pool"),
        };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        var site = input.Get<string>(SITE);
        var name = input.Get<string>(NAME);
        var path = input.Get<string>(PATH);
        var pool = input.Get<string>(POOL);

        var pathAppcmd = Context.SettingsProvider.GetSetting(Defs.Settings.PATH_APPCMD, true);
        if (string.IsNullOrEmpty(pathAppcmd))
        {
            Console.WriteLine("path for executable Appcmd not found. Operation cancelled");
            return;
        }

        var template = "add app \"/site.name:{{site}}\" /path:/{{name}} \"/physicalPath:{{path}}\" {{usePool}}";
        var usePool = !string.IsNullOrEmpty(pool) ? "/applicationPool:" + pool : "";
        var cmd = template.FormatLiquid(new { name, site, path, usePool });

        var result = UtilProcess.Invoke(pathAppcmd, cmd);
        Console.WriteLine(result);
    }
}
