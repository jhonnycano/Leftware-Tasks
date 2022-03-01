using Leftware.Tasks.Core.Model;
using Spectre.Console;
using System.Text.RegularExpressions;

namespace Leftware.Tasks.Core;

public abstract class CommonTaskBase
{
    public TaskExecutionContext Context { get; set; }

    public CommonTaskInputHelper Input { get; set; }

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