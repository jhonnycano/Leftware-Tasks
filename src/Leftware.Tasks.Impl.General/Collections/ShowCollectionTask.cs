using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.TaskParameters;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using Spectre.Console;

namespace Leftware.Tasks.Impl.General.Collections
{
    [Descriptor("General - Show collection")]
    public class ShowCollectionTask : CommonTaskBase
    {
        private const string COLLECTION = "collection";
        private readonly ICollectionProvider _collectionProvider;

        public ShowCollectionTask(ICollectionProvider collectionProvider)
        {
            _collectionProvider = collectionProvider;
        }

        public override IList<TaskParameter> GetTaskParameterDefinition()
        {
            return new List<TaskParameter>
            {
                new SelectStringTaskParameter(COLLECTION, "Collection", _collectionProvider.GetCollections())
            };
        }

        public override async Task Execute(IDictionary<string, object> input)
        {
            var col = UtilCollection.Get(input, COLLECTION, "");

            var header = _collectionProvider.GetHeader(col) ?? throw new InvalidOperationException("Collection not found");
            var tableHeader = new Table()
                .AddColumns("Type", "Schema");

            tableHeader.AddRow(header.ItemType + "", (header.Schema ?? "").Replace("[", "[[").Replace("]", "]]"));
            AnsiConsole.Write(tableHeader);

            var collectionItems = _collectionProvider.GetItems(col) ?? throw new InvalidOperationException("Collection items not found");

            var table = new Table()
                .AddColumns("Key", "Label", "Content");
            foreach(var itm in collectionItems)
            {
                table.AddRow(itm.Key, itm.Label, itm.Content.Replace("[", "[[").Replace("]", "]]"));
            }
;
            AnsiConsole.Write(table);
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
