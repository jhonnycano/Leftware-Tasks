using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.TaskParameters;
using Leftware.Tasks.Core.TaskParameters.Conditions;

namespace Leftware.Tasks.Impl.General;

[Descriptor("Data transformation - Apply liquid template")]
internal class ApplyLiquidTemplateTask : CommonTaskBase
{
    private const string TEMPLATE_SOURCE_TYPE = "template-source-type";
    private const string TEMPLATE_SOURCE = "template-source";
    private const string INPUT_SOURCE_TYPE = "input-source-type";
    private const string INPUT_SOURCE = "input-source";
    private const string OUTPUT_TYPE = "output-type";
    private const string OUTPUT = "output";

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        var sourceTypes = new[] { "File", "Inline" };
        return new List<TaskParameter>
        {
            new SelectStringTaskParameter(TEMPLATE_SOURCE_TYPE, "Template source type", sourceTypes),
            new ReadStringTaskParameter(TEMPLATE_SOURCE, "Template source")
                .When(new EqualsCondition(TEMPLATE_SOURCE_TYPE, "Inline")),
            new ReadFileTaskParameter(TEMPLATE_SOURCE, "Template source")
                .When(new EqualsCondition(TEMPLATE_SOURCE_TYPE, "File")),

            new SelectStringTaskParameter(INPUT_SOURCE_TYPE, "Input source type", sourceTypes),
            new ReadStringTaskParameter(INPUT_SOURCE, "Input source")
                .When(new EqualsCondition(INPUT_SOURCE_TYPE, "Inline")),
            new ReadFileTaskParameter(INPUT_SOURCE, "Input source")
                .When(new EqualsCondition(INPUT_SOURCE_TYPE, "File")),
            new SelectStringTaskParameter(OUTPUT_TYPE, "Input source type", sourceTypes),

            new ReadStringTaskParameter(OUTPUT, "Input source")
                .When(new EqualsCondition(OUTPUT_TYPE, "Inline")),
            new ReadFileTaskParameter(OUTPUT, "Input source")
                .When(new EqualsCondition(OUTPUT_TYPE, "File")),
        };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        var templateSourceType = input.Get<string>(TEMPLATE_SOURCE_TYPE);
        var templateSource = input.Get<string>(TEMPLATE_SOURCE);
        var inputSourceType = input.Get<string>(INPUT_SOURCE_TYPE);
        var inputSource = input.Get<string>(INPUT_SOURCE);
        var outputType = input.Get<string>(OUTPUT_TYPE);
        var output = input.Get<string>(OUTPUT);

        var template = GetString(templateSourceType, templateSource);
        var inputItem = GetString(inputSourceType, inputSource);
        var outputItem = ApplyLiquid(template, inputItem);
        WriteString(outputItem.result, outputType, output);
        WriteErrors(outputItem.errors);
    }

    private (string result, IList<string> errors) ApplyLiquid(string template, string inputItem)
    {
        return StringExtensions.ApplyLiquid(template, inputItem);
    }

    private static string GetString(string sourceType, string source)
    {
        switch(sourceType)
        {
            case "File":
                return File.ReadAllText(source);
            case "Inline":
                return source;
            default:
                throw new InvalidOperationException($"Unknown source type: {sourceType}");
        }
    }

    private void WriteString(string item, string sourceType, string source)
    {
        switch (sourceType)
        {
            case "File":
                File.WriteAllText(source, item);
                break;
            case "Inline":
                Console.WriteLine(item);
                break;
            default:
                throw new InvalidOperationException($"Unknown source type: {sourceType}");
        }
    }

    private static void WriteErrors(IList<string> errors)
    {
        if (errors == null || errors.Count <= 0) return;

        Console.WriteLine($"Following errors occurred while performing transformation:");
        foreach (var err in errors)
        {
            Console.WriteLine(err);
        }
    }
}
