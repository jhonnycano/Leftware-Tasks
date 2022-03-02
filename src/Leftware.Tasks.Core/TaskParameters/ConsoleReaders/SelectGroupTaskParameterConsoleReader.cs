namespace Leftware.Tasks.Core.TaskParameters.ConsoleReaders;

internal class SelectGroupTaskParameterConsoleReader : TaskParameterConsoleReaderBase<SelectGroupTaskParameter>
{
    private readonly ITaskParameterConsoleReaderFactory _taskParameterConsoleReaderFactory;

    public SelectGroupTaskParameterConsoleReader(ITaskParameterConsoleReaderFactory taskParameterConsoleReaderFactory)
    {
        _taskParameterConsoleReaderFactory = taskParameterConsoleReaderFactory;
    }
    public override void Read(ConsoleReadContext context, SelectGroupTaskParameter param)
    {
        foreach (var p in param.Parameters)
        {
            var reader = _taskParameterConsoleReaderFactory.GetInstance(p.Type);
            var isEvaluable = CheckEvaluable(p, context);
            if (isEvaluable) reader.Read(context, p);
            if (context.IsCanceled) return;
        }
    }

    private static bool CheckEvaluable(TaskParameter p, ConsoleReadContext context)
    {
        foreach (var condition in p.Conditions)
            if (!condition.Evaluate(context)) return false;

        return true;
    }
}
