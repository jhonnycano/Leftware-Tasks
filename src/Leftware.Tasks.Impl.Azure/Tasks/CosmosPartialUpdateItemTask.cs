using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.Model;
using Leftware.Tasks.Core.TaskParameters;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;

namespace Leftware.Tasks.Impl.Azure.Tasks;

[Descriptor("Azure Cosmos - Partial update item (apply patch operations)")]
public class CosmosPartialUpdateItemTask : CommonTaskBase
{
    private const string CONNECTION = "connection";
    private const string DATABASE = "database";
    private const string CONTAINER = "container";
    private const string ID = "id";
    private const string PARTITIONKEY = "partitionkey";
    private const string PATCHES = "patches";

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        return new List<TaskParameter>
        {
            new SelectFromCollectionTaskParameter(CONNECTION, "connection", Defs.Collections.AZURE_COSMOS_CONNECTION),
            new SelectFromCollectionTaskParameter(DATABASE, "database", Defs.Collections.AZURE_COSMOS_DATABASE, true)
                .WithDefaultValue($"->{Defs.Collections.AZURE_COSMOS_CONNECTION}|{CONNECTION}|$.Database"),
            new SelectFromCollectionTaskParameter(CONTAINER, "container", Defs.Collections.AZURE_COSMOS_CONTAINER, true)
                .WithDefaultValue($"->{Defs.Collections.AZURE_COSMOS_CONNECTION}|{CONNECTION}|$.Container"),
            new ReadStringTaskParameter(ID, "Document Id"), 
            new ReadStringTaskParameter(PARTITIONKEY, "Partition Key"), 
            new ReadFileTaskParameter(PATCHES, "File with patches to apply"), 
        };
    }

    public async override Task Execute(IDictionary<string, object> input)
    {
        var connectionKey = input.Get(CONNECTION, "");
        var database = GetCollectionValue<string>(input, DATABASE, Defs.Collections.AZURE_COSMOS_DATABASE);
        var container = GetCollectionValue<string>(input, CONTAINER, Defs.Collections.AZURE_COSMOS_CONTAINER);
        var id = input.Get(ID, "");
        var partitionKey = input.Get(PARTITIONKEY, "");
        var patchesFile = input.Get(PATCHES, "");

        var connection = Context.CollectionProvider.GetItemContentAs<CosmosConnection>(Defs.Collections.AZURE_COSMOS_CONNECTION, connectionKey!);
        if (connection == null)
        {
            UtilConsole.WriteError($"Source connection not found: {connectionKey}");
            return;
        }

        connection.Database = database;
        connection.Container = container;

        var content = File.ReadAllText(patchesFile!);
        var obj = JArray.Parse(content);
        var patches = GetPatchOperations(obj);
        var writer = new CosmosWriter(connection);
        await writer.PatchItemAsync(id!, partitionKey!, patches);
    }

    private static List<PatchOperation> GetPatchOperations(JArray arr)
    {
        var result = new List<PatchOperation>();
        foreach(var itm in arr)
        {
            if (itm.Type != JTokenType.Object) continue;
            var obj = (JObject)itm;
            var operation = obj.Value<string>("op");
            switch(operation)
            {
                case "add":
                    Add(result, obj);
                    break;
                case "set":
                    Set(result, obj);
                    break;
                case "replace":
                    Replace(result, obj);
                    break;
                case "remove":
                    Remove(result, obj);
                    break;
            }
        }
        return result;
    }

    private static void Add(IList<PatchOperation> list, JObject obj)
    {
        var path = obj.Value<string>("path");
        var value = obj.Value<string>("value");
        list.Add(PatchOperation.Add(path, value));
    }
    private static void Set(IList<PatchOperation> list, JObject obj)
    {
        var path = obj.Value<string>("path");
        var value = obj.Value<string>("value");
        list.Add(PatchOperation.Set(path, value));
    }
    private static void Replace(IList<PatchOperation> list, JObject obj)
    {
        var path = obj.Value<string>("path");
        var value = obj.Value<string>("value");
        list.Add(PatchOperation.Replace(path, value));
    }
    private static void Remove(IList<PatchOperation> list, JObject obj)
    {
        var path = obj.Value<string>("path");
        list.Add(PatchOperation.Remove(path));
    }
}
