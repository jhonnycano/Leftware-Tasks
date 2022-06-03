namespace Leftware.Tasks.Core.TaskParameters;

public class ReadFileTaskParameter : TaskParameter<string>
{
    public IList<(Func<string, bool> validator, string message)> Validations { get; private set; }

    public ReadFileTaskParameter(string name, string label) : this(name, label, true)
    {
    }

    public ReadFileTaskParameter(string name, string label, bool shouldExist) : base(name, label)
    {
        Type = TaskParameterType.ReadFile;
        ShouldExist = shouldExist;
    }

    public bool ShouldExist { get; private set; }

    public ReadFileTaskParameter WithValidation(Func<string, bool> validation, string message)
    {
        if (Validations == null) Validations = new List<(Func<string, bool>, string)>();
        Validations.Add((validation, message));
        return this;
    }

    public ReadFileTaskParameter WithSchema<T>()
    {
        var messageSchema = UtilJsonSchema.GetJsonSchemaForType<T>();
        if (Validations == null) Validations = new List<(Func<string, bool>, string)>();
        Validations.Add((i => {
            var content = File.ReadAllText(i);
            return UtilJsonSchema.IsValidJsonForSchema(content, messageSchema);
        }, "Schema validation failed"));
        return this;
    }

}
