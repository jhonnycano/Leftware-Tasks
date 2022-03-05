namespace Leftware.Tasks.Core.TaskParameters;

public class SelectFromCollectionTaskParameter : TaskParameter<string>, IShouldExist
{
    public SelectFromCollectionTaskParameter(string name, string label, string collection,
        bool allowManualEntry = false) : base(name, label)
    {
        Type = TaskParameterType.SelectFromCollection;
        Collection = collection;
        DefaultKey = default;
        AllowManualEntry = allowManualEntry;
        ShouldExist = true;
    }

    public string Collection { get; private set; }
    public string? DefaultKey { get; private set; }
    public bool AllowManualEntry { get; private set; }
    public bool ShouldExist { get; set; }

    public SelectFromCollectionTaskParameter WithDefaultKey(string key)
    {
        DefaultKey = key;
        return this;
    }

    public SelectFromCollectionTaskParameter WithShouldExist(bool shouldExist)
    {
        ShouldExist = shouldExist;
        return this;
    }
}
