using Leftware.Common;

namespace Leftware.Tasks.Core.TaskParameters.ConsoleReaders;

internal class ReadFolderTaskParameterConsoleReader : TaskParameterConsoleReaderBase<ReadFolderTaskParameter>
{
    public override void Read(ConsoleReadContext context, ReadFolderTaskParameter param)
    {
        var value = UtilConsole.ReadFolder(param.Label, param.ShouldExist);
        if (string.IsNullOrEmpty(value))
        {
            context.IsCanceled = true;
            return;
        }

        context[param.Name] = value;
    }
}
