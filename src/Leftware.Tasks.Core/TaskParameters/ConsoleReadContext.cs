namespace Leftware.Tasks.Core.TaskParameters;

public class ConsoleReadContext
{
    public ConsoleReadContext()
    {
        Values = new Dictionary<string, object>();
        IsCanceled = false;
    }

    public IDictionary<string, object> Values { get; set; }
    public bool IsCanceled { get; set; }

    public object this[string key] {
        get => Values[key];
        set => Values[key] = value;
    }
}
