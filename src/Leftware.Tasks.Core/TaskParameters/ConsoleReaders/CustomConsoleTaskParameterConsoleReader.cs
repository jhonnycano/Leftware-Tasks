namespace Leftware.Tasks.Core.TaskParameters.ConsoleReaders;

internal class CustomConsoleTaskParameterConsoleReader : TaskParameterConsoleReaderBase<CustomConsoleTaskParameter>
{
    public override void Read(ConsoleReadContext context, CustomConsoleTaskParameter param)
    {
        var isCanceled = param.CustomReadFunction(context);
        if (isCanceled)
        {
            context.IsCanceled = true;
            return;
        }
    }
}
