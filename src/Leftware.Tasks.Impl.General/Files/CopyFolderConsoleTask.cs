using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.TaskParameters;

namespace Leftware.Tasks.Impl.General.Files;

/*
[Descriptor("FileSystem - Copy folder")]
internal class CopyFolderConsoleTask : CommonTaskBase
{
    private readonly IRoboCopyTask _roboCopyTask;
    private const string SOURCE = "source";
    private const string TARGET = "target";

    public CopyFolderConsoleTask(IRoboCopyTask roboCopyTask)
    {
        _console = console;
        _roboCopyTask = roboCopyTask;
    }

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        return new List<TaskParameter>
        {
            new SelectFromCollectionTaskParameter(SOURCE, "Source folder", Defs.Collections.FAVORITE_FOLDER, true),
            new SelectFromCollectionTaskParameter(TARGET, "Target folder", Defs.Collections.FAVORITE_FOLDER, true),
        };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        var source = input.Get<string>(SOURCE);
        var target = input.Get<string>(TARGET);

        var robocopyTaskInput = new RoboCopyTaskInput
        {
            Source = source,
            Destination = target,
            Files = "*.*",
            Options = "/E /A-:R /NFL /NDL"
        };
        var result = _roboCopyTask.Execute(robocopyTaskInput);
        Console.WriteLine("Result: {0}", result);
    }
}
*/
