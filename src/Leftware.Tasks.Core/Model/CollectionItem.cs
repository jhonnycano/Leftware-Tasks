using Leftware.Common;
using Newtonsoft.Json;

namespace Leftware.Tasks.Core.Model;

public class CollectionItem
{
    public CollectionItem()
    {
        Collection = "";
        Key = "";
        Label = "";
        Content = "";
    }

    public CollectionItem(
    string collection,
    string key,
    string label,
    string content
)
    {
        Collection = collection;
        Key = key;
        Label = label;
        Content = content;
    }

    public string Collection { get; set; }
    public string Key { get; set; }
    public string Label { get; set; }
    public string Content { get; set; }

    public T As<T>()
    {
        if (typeof(T) == typeof(string))
        {
            return UtilConvert.ConvertTo<T>(Content)
                ?? throw new InvalidOperationException($"Could not convert content to string for item {Collection}.{Key}");
        }
        var result = JsonConvert.DeserializeObject<T>(Content)
            ?? throw new InvalidOperationException($"Could not deserialize content for collection item {Collection}.{Key} into type {typeof(T).Name}");
        return result;
    }
}

