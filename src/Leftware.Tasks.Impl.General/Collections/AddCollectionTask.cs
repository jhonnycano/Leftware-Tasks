using Leftware.Common;
using Leftware.Tasks.Core;
using NJsonSchema;
using Spectre.Console;

namespace Leftware.Tasks.Impl.General.Collections
{
    [Descriptor("General - Add collection")]
    public class AddCollectionTask : CommonTaskBase
    {
        private readonly ICollectionProvider _collectionProvider;

        public AddCollectionTask(ICollectionProvider collectionProvider)
        {
            _collectionProvider = collectionProvider;
        }

        public override async Task<IDictionary<string, object>?> GetTaskInput()
        {
            var dic = GetEmptyTaskInput();
            var name = AnsiConsole.Prompt(
                new TextPrompt<string>("[green]Collection name[/]")
                .AllowEmpty()
                );
            dic["name"] = name;

            var enumNames = Enum.GetNames(typeof(CollectionItemType));
            var itemTypeString = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .Title("[green]Collection name[/]")
                .AddChoices(enumNames)
                );

            var itemType = UtilEnum.Get<CollectionItemType>(itemTypeString);
            dic["type"] = itemType;

            if (itemType == CollectionItemType.JsonObject)
            {
                var schema = AnsiConsole.Prompt(
                    new TextPrompt<string>("[green]Schema[/]")
                    .AllowEmpty()
                    .Validate(ValidateSchema, "Invalid schema")
                    );
                dic["schema"] = schema;
            }

            return dic;
        }

        public override async Task Execute(IDictionary<string, object> input)
        {
            var name = UtilCollection.Get(input, "name", "");
            var type = UtilCollection.Get(input, "type", CollectionItemType.None);
            var schema = UtilCollection.Get(input, "schema", "");

            if (type == CollectionItemType.None)
            {
                AnsiConsole.Markup("[red]ItemType not found[/]");
                return;
            } 
            else if (type == CollectionItemType.String)
            {
                schema = null;
            }

            await _collectionProvider.AddCollectionAsync(name, type, schema);
        }

        private bool ValidateSchema(string arg)
        {
            try
            {
                var schema = JsonSchema.FromJsonAsync(arg).GetAwaiter().GetResult();
                return schema.IsObject || schema.IsArray;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
