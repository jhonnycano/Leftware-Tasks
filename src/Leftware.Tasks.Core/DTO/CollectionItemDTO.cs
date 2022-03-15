using Leftware.Tasks.Core.Model;
using Newtonsoft.Json.Linq;

namespace Leftware.Tasks.Core.DTO;

public class CollectionItemDTO
{
    public string Collection { get; set; }
    public string Key { get; set; }
    public string Label { get; set; }
    public object Content { get; set; }

    public CollectionItemDTO()
    {
    }

    public CollectionItemDTO(CollectionHeader header, CollectionItem item)
    {
        Collection = item.Collection;
        Key = item.Key;
        Label = item.Label;
        Content = header.ItemType == CollectionItemType.JsonObject ? JObject.Parse(item.Content) : item.Content;
    }
}

