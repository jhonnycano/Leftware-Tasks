using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.TaskParameters;

namespace Leftware.Tasks.Impl.General.WebServers.IIS;

[Descriptor("IIS - Remove IIS app")]
internal class RemoveIISAppTask : CommonTaskBase
{
    private const string SITE = "site";
    private const string NAME = "name";

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        return new List<TaskParameter>
        {
            new ReadStringTaskParameter(SITE, "host site").WithDefaultValue("Default Web Site"),
            new ReadStringTaskParameter(NAME, "app name"),
        };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        var site = input.Get<string>(SITE);
        var name = input.Get<string>(NAME);

        var appcmdPath = Context.SettingsProvider.GetSetting(Defs.Settings.PATH_APPCMD, true);
        if (string.IsNullOrEmpty(appcmdPath))
        {
            Console.WriteLine("path for executable Appcmd not found. Operation cancelled");
            return;
        }

        var template = "delete app \"/app.name:{{site}}/{{name}}";
        var cmd = template.FormatLiquid(new { name, site });

        var result = UtilProcess.Invoke(appcmdPath, cmd);
        Console.WriteLine(result);
    }
}
