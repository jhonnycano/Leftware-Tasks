using Leftware.Tasks.Core.Model;
using Newtonsoft.Json.Linq;

namespace Leftware.Tasks.Core.DTO;

public class CollectionHeaderDTO
{
    public string Name { get; set; }
    public CollectionItemType ItemType { get; set; }
    public object? Schema { get; set; }

    public CollectionHeaderDTO()
    {

    }

    public CollectionHeaderDTO(CollectionHeader header)
    {
        Name = header.Name;
        ItemType = header.ItemType.Value;
        Schema = header.Schema == null ? default : JObject.Parse(header.Schema);
    }
}

