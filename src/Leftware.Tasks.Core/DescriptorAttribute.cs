namespace Leftware.Tasks.Core;

[AttributeUsage(AttributeTargets.Class)]
public class DescriptorAttribute : Attribute
{
    public string Name { get; set; }
    public int SortOrder { get; set; }
    public bool ConfirmBeforeExecution { get; set; }
    public bool Enabled { get; set; }

    public DescriptorAttribute(
        string name,
        int sortOrder = 0,
        bool confirmBeforeExecution = true
        )
    {
        Name = name;
        SortOrder = sortOrder;
        ConfirmBeforeExecution = confirmBeforeExecution;
        Enabled = true;
    }
}