using Leftware.Tasks.Core;
using Leftware.Tasks.Core.TaskParameters;

namespace Leftware.Tasks.Impl.General
{
    [Descriptor("General - Test input")]
    public class TestInputTask : CommonTaskBase
    {
        private const string VARIABLE = "variable";
        private const string VALUE = "value";

        public override IList<TaskParameter> GetTaskParameterDefinition()
        {
            return new List<TaskParameter>
            {
                new ReadBoolTaskParameter("read-bool", "Read bool"),
                new ReadIntegerTaskParameter("read-int", "Read int with range 10, 20").WithRange(10, 20),
                new ReadStringTaskParameter("read-string", "Read string").WithRegex("^\\w+$"),
                new ReadFileTaskParameter("read-file", "Read file"),
                new ReadFolderTaskParameter("read-folder", "Read folder", true),
            };
        }

        public override async Task Execute(IDictionary<string, object> input)
        {
            return;
        }
    }
}