using Newtonsoft.Json;
using NJsonSchema;

namespace Leftware.Tasks.Core
{
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
    }
}
