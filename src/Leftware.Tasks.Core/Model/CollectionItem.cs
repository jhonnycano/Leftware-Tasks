using Newtonsoft.Json;

namespace Leftware.Tasks.Core.Model;

public class CollectionItem
{
    public CollectionItem()
    {

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

    public T? As<T>()
    {
        return JsonConvert.DeserializeObject<T>(Content);
    }
}

