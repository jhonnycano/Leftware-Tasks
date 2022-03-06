using Leftware.Common;
using Leftware.Tasks.Core.TaskParameters;

namespace Leftware.Tasks.Core;

public abstract class CommonTaskBase
{
    public TaskExecutionContext Context { get; set; }

    public CommonTaskInputHelper Input { get; set; }

    public async virtual Task<IDictionary<string, object>?> GetTaskInput()
    {
        return await Task.FromResult(default(IDictionary<string, object>));
    }

    public virtual IList<TaskParameter> GetTaskParameterDefinition()
    {
        return null;
    }

    public abstract Task Execute(IDictionary<string, object> input);

    protected static IDictionary<string, object> GetEmptyTaskInput()
    {
        return new Dictionary<string, object>();
    }

    protected T GetCollectionValue<T>(IDictionary<string, object> dic, string key, string collection)
    {
        var item = UtilCollection.Get(dic, key, "");
        if (item == Defs.USE_AS_VALUE) {
            var itemValue = UtilCollection.Get(dic, $"{key}__$rawValue", "");
            return UtilConvert.ConvertTo<T>(itemValue);
        }
        
        var itemContent = Context.CollectionProvider.GetItemContentAs<T>(collection, item);
        return itemContent;
    }
}