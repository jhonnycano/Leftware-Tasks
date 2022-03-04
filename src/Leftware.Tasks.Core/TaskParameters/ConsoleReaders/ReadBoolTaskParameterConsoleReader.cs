using Leftware.Common;
using Spectre.Console;

namespace Leftware.Tasks.Core.TaskParameters.ConsoleReaders;

internal class ReadBoolTaskParameterConsoleReader : TaskParameterConsoleReaderBase<ReadBoolTaskParameter>
{
    public override void Read(ConsoleReadContext context, ReadBoolTaskParameter param)
    {
        var value = UtilConsole.ReadBool(param.Label, param.DefaultValue);
        if (value == null)
        {
            context.IsCanceled = true;
            return;
        }

        context[param.Name] = value.Value;
    }
}
