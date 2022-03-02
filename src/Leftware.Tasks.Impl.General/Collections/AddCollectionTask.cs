using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.TaskParameters;
using Leftware.Tasks.Core.TaskParameters.Conditions;
using NJsonSchema;
using Spectre.Console;

namespace Leftware.Tasks.Impl.General.Collections;

[Descriptor("General - Add collection")]
public class AddCollectionTask : CommonTaskBase
{
    private const string NAME = "name";
    private const string TYPE = "type";
    private const string SCHEMA = "schema";
    private readonly ICollectionProvider _collectionProvider;

    public AddCollectionTask(ICollectionProvider collectionProvider)
    {
        _collectionProvider = collectionProvider;
    }

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        return new List<TaskParameter>
            {
                new ReadStringTaskParameter(NAME, "Collection name")
                    .WithRegex("[A-Za-z\\-_]+")
                    .WithLengthRange(4, 80),
                new SelectEnumTaskParameter(TYPE, "Collection type", typeof(CollectionItemType))
                    .SkipValues(CollectionItemType.None),
                new ReadStringTaskParameter(SCHEMA, "Collection schema")
                    .WithValidation(ValidateSchema, "Invalid schema")
                    .When(new EqualsCondition(TYPE, CollectionItemType.JsonObject)),
            };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        var name = UtilCollection.Get(input, NAME, "");
        var type = UtilCollection.Get(input, TYPE, CollectionItemType.None);
        var schema = UtilCollection.Get(input, SCHEMA, "");

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
