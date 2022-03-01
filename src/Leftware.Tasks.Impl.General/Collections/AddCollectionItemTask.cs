using Leftware.Common;
using Leftware.Tasks.Core;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using Spectre.Console;

namespace Leftware.Tasks.Impl.General.Collections
{
    [Descriptor("General - Add collection item")]
    public class AddCollectionItemTask : CommonTaskBase
    {
        private readonly ICollectionProvider _collectionProvider;

        public AddCollectionItemTask(ICollectionProvider collectionProvider)
        {
            _collectionProvider = collectionProvider;
        }

        public override async Task<IDictionary<string, object>?> GetTaskInput()
        {
            var dic = GetEmptyTaskInput();

            var cols = _collectionProvider.GetCollections();
            var collection = SelectOption(dic, "col", "Collection", cols);
            if (collection == null) return null;

            var collectionHeader = _collectionProvider.GetHeader(collection) ?? throw new InvalidOperationException("Collection not found");

            if (!GetStringValidRegex(dic, "key", "Key", null, "\\w+")) return null;
            if (!GetString(dic, "label", "Label")) return null;

            var content = AnsiConsole.Prompt(
                new TextPrompt<string>("[green]Content[/]")
                    .Validate(s => ValidateConformsToSchema(s, collectionHeader.Schema), "Content does not conform to schema of collection")
                );
            dic["content"] = content;

            return dic;
        }

        public override async Task Execute(IDictionary<string, object> input)
        {
            var col = UtilCollection.Get(input, "col", "");
            var key = UtilCollection.Get(input, "key", "");
            var label = UtilCollection.Get(input, "label", "");
            var content = UtilCollection.Get(input, "content", "");

            var collectionHeader = _collectionProvider.GetHeader(col) ?? throw new InvalidOperationException("Collection not found");

            if (collectionHeader.ItemType == CollectionItemType.JsonObject)
            {
                if (!ValidateConformsToSchema(content, collectionHeader.Schema))
                {
                    UtilConsole.WriteError("Content does not conform to schema of collection");
                    return;
                }
            }

            await _collectionProvider.AddItemAsync(col, key, label, content);
        }

        private bool ValidateConformsToSchema(string json, string? schemaJson)
        {
            try
            {
                if (schemaJson == null) return true;

                var schema = JsonSchema.FromJsonAsync(schemaJson).GetAwaiter().GetResult();
                var token = JToken.Parse(json);
                var errors = schema.Validate(token);
                var errorsFound = errors != null && errors.Count > 0;
                if (errorsFound)
                {
                    AnsiConsole.MarkupLine("[red]ERRORS FOUND[/]");

                    foreach (var err in errors)
                    {
                        Console.WriteLine(err.ToString());
                    }
                }
                return !errorsFound;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
