using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.TaskParameters;
using Leftware.Tasks.Core.TaskParameters.Conditions;
using Spectre.Console;

namespace Leftware.Tasks.Impl.General.Files;

[Descriptor("Files - Validate JSON Schema")]
internal class ValidateJsonSchemaTask : CommonTaskBase
{
    private const string CONTENT_SOURCE_TYPE = "content-source-type";
    private const string CONTENT_SOURCE = "content-source";
    private const string SCHEMA_SOURCE_TYPE = "schema-source-type";
    private const string SCHEMA_SOURCE = "schema-source";
    private const string OUTPUT_TYPE = "output-type";
    private const string OUTPUT = "output";

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        var sourceTypes = new[] { "File", "Inline" };
        return new List<TaskParameter>
        {
            new SelectStringTaskParameter(CONTENT_SOURCE_TYPE, "Content source type", sourceTypes),
            new ReadStringTaskParameter(CONTENT_SOURCE, "Content source")
                .When(new EqualsCondition(CONTENT_SOURCE_TYPE, "Inline")),
            new ReadFileTaskParameter(CONTENT_SOURCE, "Content file")
                .When(new EqualsCondition(CONTENT_SOURCE_TYPE, "File")),

            new SelectStringTaskParameter(SCHEMA_SOURCE_TYPE, "Schema source type", sourceTypes),
            new ReadStringTaskParameter(SCHEMA_SOURCE, "Schema source")
                .When(new EqualsCondition(SCHEMA_SOURCE_TYPE, "Inline")),
            new ReadFileTaskParameter(SCHEMA_SOURCE, "Schema file")
                .When(new EqualsCondition(SCHEMA_SOURCE_TYPE, "File")),

            /*
            new SelectStringTaskParameter(OUTPUT_TYPE, "Output source type", sourceTypes),
            new ReadFileTaskParameter(OUTPUT, "Output file")
                .When(new EqualsCondition(OUTPUT_TYPE, "File")),
            */
        };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        var contentSourceType = input.Get<string>(CONTENT_SOURCE_TYPE);
        var contentSource = input.Get<string>(CONTENT_SOURCE);
        var schemaSourceType = input.Get<string>(SCHEMA_SOURCE_TYPE);
        var schemaSource = input.Get<string>(SCHEMA_SOURCE);
        //var outputType = input.Get<string>(OUTPUT_TYPE);
        //var output = input.Get<string>(OUTPUT);

        var content = GetString(contentSourceType, contentSource);
        var schema = GetString(schemaSourceType, schemaSource);
        EvaluateSchema(content, schema);

    }

    private void EvaluateSchema(string content, string schema)
    {
        try
        {
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();

            var errorsTask = UtilJsonSchema.Validate(content, schema);
            errorsTask.Wait();
            
            var errors = errorsTask.Result;
            if (errors.Count == 0)
            {
                Context.StatusContext?.Status("Schema validated successfully");
                return;
            }

            Context.StatusContext?.Status("Schema validated with errors");

            foreach (var error in errors)
            {
                AnsiConsole.WriteLine(error);
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
        }
    }

    private static string GetString(string? sourceType, string? source)
    {
        switch (sourceType)
        {
            case "File":
                return File.ReadAllText(source);
            case "Inline":
                return source;
            default:
                throw new InvalidOperationException($"Unknown source type: {sourceType}");
        }
    }
}
