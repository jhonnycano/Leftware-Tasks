using Leftware.Tasks.Core;
using Leftware.Tasks.Core.TaskParameters;

namespace Leftware.Tasks.Impl.General;

[Descriptor("General - Test input")]
public class TestInputTask : CommonTaskBase
{
    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        return new List<TaskParameter>
        {
            //new ReadBoolTaskParameter("read-bool", "Read bool"),
            //new ReadIntegerTaskParameter("read-int", "Read int with range 10, 20").WithRange(10, 20),
            //new ReadStringTaskParameter("read-string", "Read string").WithRegex("^\\w+$"),
            //new ReadPasswordTaskParameter("read-password", "Read password", 6, 40),
            //new ReadFileTaskParameter("read-file", "Read file"),
            //new ReadFolderTaskParameter("read-folder", "Read folder", true),
            new SelectEnumTaskParameter("select-enum", "Select enum", typeof(DriveType)),
            new ReadStringTaskParameter("read-string", "Read string").WithRegex("^\\w+$"),
        };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        return;
    }
}
