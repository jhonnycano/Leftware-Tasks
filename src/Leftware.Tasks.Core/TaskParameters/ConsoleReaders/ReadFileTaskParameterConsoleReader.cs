using Leftware.Common;

namespace Leftware.Tasks.Core.TaskParameters.ConsoleReaders;

internal class ReadFileTaskParameterConsoleReader : TaskParameterConsoleReaderBase<ReadFileTaskParameter>
{
    public override void Read(ConsoleReadContext context, ReadFileTaskParameter param)
    {
        var value = UtilConsole.ReadFile(param.Label, param.ShouldExist);
        if (string.IsNullOrEmpty(value))
        {
            context.IsCanceled = true;
            return;
        }

        context[param.Name] = value;
    }
}
