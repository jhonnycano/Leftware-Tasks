using Leftware.Common;
using Leftware.Tasks.Core;

namespace Leftware.Tasks.Impl.General
{
    [Descriptor("General - Set context variable")]
    public class SetContextVariableTask : CommonTaskBase
    {
        public override async Task<IDictionary<string, object>?> GetTaskInput()
        {
            var dic = GetEmptyTaskInput();

            if (!Input.GetStringValidRegex(dic, "variable", "Variable name", null, "[A-Za-z\\-_]+")) return null;
            if (!Input.GetString(dic, "value", "Variable value", "")) return null;
            return dic;
        }
        public override async Task Execute(IDictionary<string, object> input)
        {
            var name = input.Get("variable", "");
            var value = input.Get("value", "");

            Context.ExtendedInfo[name] = value;
        }
    }
}