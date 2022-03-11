using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.TaskParameters;

namespace Leftware.Tasks.Impl.General;

[Descriptor("Execute - Execute cli command")]
internal class ExecuteCommandTask : CommonTaskBase
{
    private const string PATH = "path";
    private const string ARGS = "args";

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        return new List<TaskParameter>
        {
            new ReadFileTaskParameter(PATH, "Script path"),
            new ReadStringTaskParameter(ARGS, "Script arguments"),
        };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        var path = input.Get<string>(PATH);
        var args = input.Get<string>(ARGS);

        var output = UtilProcess.Invoke(path, args);
        Console.WriteLine(output);
    }
}
