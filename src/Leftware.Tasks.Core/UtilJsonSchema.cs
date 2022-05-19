using Newtonsoft.Json;
using NJsonSchema;

namespace Leftware.Tasks.Core;

public static class UtilJsonSchema
{
    public static string GetJsonSchemaForType<T>()
    {
        var schema = JsonSchema.FromType<T>();
        var schemaData = schema.ToJson(Formatting.None);
        return schemaData;
    }

    public static bool IsValidJsonForSchema(string json, string schemaJson)
    {
        var schema = JsonSchema.FromJsonAsync(schemaJson).GetAwaiter().GetResult();
        var errors = schema.Validate(json);
        return !errors.Any();
    }

    public static async Task<IList<string>> Validate(string json, string schemaJson)
    {
        var schema = await JsonSchema.FromJsonAsync(schemaJson);
        var errors = schema.Validate(json);
        var result = errors
            .Select(e => $"Path: {e.Path}, LineNumber: {e.LineNumber}, LinePosition: {e.LinePosition}, Property: {e.Property}, Kind: {e.Kind}")
            .ToList();
        return result;
    }
}
