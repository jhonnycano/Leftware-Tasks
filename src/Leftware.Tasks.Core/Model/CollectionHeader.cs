namespace Leftware.Tasks.Core.Model;

public class CollectionHeader
{

    public CollectionHeader()
    {

    }

    public CollectionHeader(
        string name,
        CollectionItemType itemType,
        string? schema = null
        )
    {
        Name = name;
        ItemType = itemType;
        Schema = schema;
    }

    public string? Name { get; set; }
    public CollectionItemType? ItemType { get; set; }
    public string? Schema { get; set; }
}
