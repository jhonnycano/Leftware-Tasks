namespace Leftware.Tasks.Core.TaskParameters;

public class ReadStringTaskParameter : TaskParameter<string>
{
    public ReadStringTaskParameter(string name, string label) : base(name, label)
    {
        Type = TaskParameterType.ReadString;
        MinLength = 0;
        MaxLength = int.MaxValue;
        AllowEmptyValue = false;
        CancelString = "?";
        RegularExpression = null;
    }

    public int MinLength { get; private set; }
    public int MaxLength { get; private set; }
    public bool AllowEmptyValue { get; private set; }
    public string CancelString { get; private set; }
    public string? RegularExpression { get; private set; }
    public IList<(Func<string, bool> validator, string message)> Validations { get; private set; }
    public IList<(Func<string, ConsoleReadContext, bool> validator, string message)> ReadContextValidations { get; private set; }

    public ReadStringTaskParameter WithRegex(string regex)
    {
        RegularExpression = regex;
        return this;
    }

    public ReadStringTaskParameter WithLengthRange(int min, int max)
    {
        MinLength = min;
        MaxLength = max;
        return this;
    }

    public ReadStringTaskParameter AllowEmpty()
    {
        AllowEmptyValue = true;
        return this;
    }

    public ReadStringTaskParameter WithValidation(Func<string, bool> validation, string message)
    {
        if (Validations == null) Validations = new List<(Func<string, bool>,string)>();
        Validations.Add((validation, message));
        return this;
    }

    public ReadStringTaskParameter WithSchema<T>()
    {
        var messageSchema = UtilJsonSchema.GetJsonSchemaForType<T>();
        if (Validations == null) Validations = new List<(Func<string, bool>, string)>();
        Validations.Add((i => UtilJsonSchema.IsValidJsonForSchema(i, messageSchema), "Schema validation failed"));
        return this;
    }

    public ReadStringTaskParameter WithReadContextValidation(Func<string, ConsoleReadContext, bool> validation, string message)
    {
        if (ReadContextValidations == null) ReadContextValidations = new List<(Func<string, ConsoleReadContext, bool>, string)>();
        ReadContextValidations.Add((validation, message));
        return this;
    }
}
