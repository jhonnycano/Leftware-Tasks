using Leftware.Tasks.Core;

namespace Leftware.Tasks.Impl.General
{
    [Descriptor("Exit", 999)]
    public class FinalizeApplicationTask : CommonTaskBase
    {
        public override async Task<IDictionary<string, object>?> GetTaskInput()
        {
            return GetEmptyTaskInput();
        }
        public override async Task Execute(IDictionary<string, object> input)
        {
            throw new UserExitException();
        }
    }
}