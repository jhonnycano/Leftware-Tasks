using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.TaskParameters;

namespace Leftware.Tasks.Impl.General
{
    [Descriptor("General - Set context variable")]
    public class SetContextVariableTask : CommonTaskBase
    {
        private const string VARIABLE = "variable";
        private const string VALUE = "value";

        public override IList<TaskParameter> GetTaskParameterDefinition()
        {
            return new List<TaskParameter>
            {
                new ReadStringTaskParameter(VARIABLE, "Variable name")
                    .WithRegex("[A-Za-z\\-_]+")
                    .WithLengthRange(4, 80),
                new ReadStringTaskParameter(VALUE, "Variable value"),
            };
        }

        public override async Task Execute(IDictionary<string, object> input)
        {
            var name = input.Get("variable", "");
            var value = input.Get("value", "");

            Context.ExtendedInfo[name] = value;
        }
    }
}