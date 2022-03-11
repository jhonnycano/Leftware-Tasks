using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.TaskParameters;

namespace Leftware.Tasks.Impl.General.WebServers.IIS;

[Descriptor("IIS - Serve folder")]
internal class ServeFolderIISTask : CommonTaskBase
{
    private const string PATH = "path";
    private const string PORT = "port";

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        return new List<TaskParameter>
        {
            new SelectFromCollectionTaskParameter(PATH, "Folder to serve", Defs.Collections.SERVE_FOLDER),
            new ReadIntegerTaskParameter(PORT, "Port").WithRange(1000, 800000).WithDefaultValue(8000),
        };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        var path = input.Get<string>(PATH);
        var port = input.Get<int>(PORT);

        var pathIISExpress = Context.SettingsProvider.GetSetting(Defs.Settings.PATH_IIS_EXPRESS, true);
        if (string.IsNullOrEmpty(pathIISExpress))
        {
            Console.WriteLine("path for executable IIS Express not found. Operation cancelled");
            return;
        }

        var template = "/path:{{ path }} /port:{{ port }}";
        var cmd = template.FormatLiquid(new { path, port });

        UtilProcess.InvokeNoShell(pathIISExpress, cmd);
    }
}
