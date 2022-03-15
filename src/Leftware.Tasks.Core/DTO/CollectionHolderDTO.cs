namespace Leftware.Tasks.Core.DTO;

public class CollectionHolderDTO
{
    public CollectionHeaderDTO Header { get; set; }
    public IList<CollectionItemDTO> Items { get; set; }
}
