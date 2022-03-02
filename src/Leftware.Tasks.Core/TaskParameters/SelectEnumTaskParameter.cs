using Newtonsoft.Json;

namespace Leftware.Tasks.Core.TaskParameters;

public class SelectEnumTaskParameter : TaskParameter<object>
{
    public SelectEnumTaskParameter(string name, string label, Type enumType) : base(name, label)
    {
        Type = TaskParameterType.SelectEnum;
        EnumType = enumType;
        CancelIfDefault = true;
    }

    [JsonIgnore]
    public Type EnumType { get; set; }

    public IList<string> EnumValues {
        get {
            return Enum.GetNames(EnumType);
        }
    }

    public bool CancelIfDefault { get; set; }
    public IList<string> ValuesToSkip { get; private set; }

    public SelectEnumTaskParameter SkipValues<T>(params T[] valuesToSkip)
    {
        if (ValuesToSkip == null) ValuesToSkip = new List<string>();
        foreach (var valueToSkip in valuesToSkip)
        {
            var stringValue = valueToSkip?.ToString();
            if (stringValue is null) continue;
            ValuesToSkip.Add(stringValue);
        }

        return this;
    }
}
