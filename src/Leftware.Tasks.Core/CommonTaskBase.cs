namespace Leftware.Tasks.Core;

public abstract class CommonTaskBase
{
    public TaskExecutionContext? Context { get; set; }

    public async virtual Task<IDictionary<string, object>?> GetTaskInput()
    {
        return await Task.FromResult(default(IDictionary<string, object>));
    }

    public abstract Task Execute(IDictionary<string, object> input);

    protected static IDictionary<string, object> GetEmptyTaskInput()
    {
        return new Dictionary<string, object>();
    }
}