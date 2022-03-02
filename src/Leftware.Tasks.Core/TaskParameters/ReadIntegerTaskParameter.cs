namespace Leftware.Tasks.Core.TaskParameters;

public class ReadIntegerTaskParameter : TaskParameter<int>
{
    public ReadIntegerTaskParameter(string name, string label) : base(
        name, label)
    {
        Type = TaskParameterType.ReadInteger;
        MinValue = 0;
        MaxValue = int.MaxValue;
    }

    public int MinValue { get; private set; }
    public int MaxValue { get; private set; }
    public int? CurrentValue { get; private set; }

    public ReadIntegerTaskParameter WithCurrentValue(int currentValue)
    {
        CurrentValue = currentValue;
        return this;
    }

    public ReadIntegerTaskParameter WithRange(int min, int max)
    {
        MinValue = min;
        MaxValue = max;
        return this;
    }

}
