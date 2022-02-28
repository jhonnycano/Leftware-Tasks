namespace Leftware.Tasks.Core;

[AttributeUsage(AttributeTargets.Class)]
public class DescriptorAttribute : Attribute
{
    public string Name { get; set; }
    public int SortOrder { get; set; }
    //public bool UseDefinitionForInput { get; }

    public DescriptorAttribute(
        string name,
        int sortOrder = 0
        )
    {
        Name = name;
        SortOrder = sortOrder;
        //UseDefinitionForInput = false;
    }
}