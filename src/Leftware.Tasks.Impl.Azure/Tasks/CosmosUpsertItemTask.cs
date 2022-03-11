using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.Model;
using Leftware.Tasks.Core.TaskParameters;
using Newtonsoft.Json.Linq;

namespace Leftware.Tasks.Impl.Azure.Tasks;

[Descriptor("Azure Cosmos - Upsert item")]
public class CosmosUpsertItemTask : CommonTaskBase
{
    private const string CONNECTION = "connection";
    private const string DATABASE = "database";
    private const string CONTAINER = "container";
    private const string FILE = "file";

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        return new List<TaskParameter>
        {
            new SelectFromCollectionTaskParameter(CONNECTION, "connection", Defs.Collections.AZURE_COSMOS_CONNECTION),
            new SelectFromCollectionTaskParameter(DATABASE, "database", Defs.Collections.AZURE_COSMOS_DATABASE, true)
                .WithDefaultValue($"->{Defs.Collections.AZURE_COSMOS_CONNECTION}|{CONNECTION}|$.Database"),
            new SelectFromCollectionTaskParameter(CONTAINER, "container", Defs.Collections.AZURE_COSMOS_CONTAINER, true)
                .WithDefaultValue($"->{Defs.Collections.AZURE_COSMOS_CONNECTION}|{CONNECTION}|$.Container"),
            new ReadFileTaskParameter(FILE, "file"),
        };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        var connectionKey = input.Get(CONNECTION, "");
        var file = input.Get(FILE, "");
        /*
        var sourceDatabase = input.Get("source-database", "");
        var sourceContainer = input.Get("source-container", "");
        var connectionTargetKey = input.Get("target-connection", "");
        var targetDatabase = input.Get("target-database", "");
        var targetContainer = input.Get("target-container", "");
        */

        var connection = Context.CollectionProvider.GetItemContentAs<CosmosConnection>(Defs.Collections.AZURE_COSMOS_CONNECTION, connectionKey!);
        if (connection == null)
        {
            UtilConsole.WriteError($"Source connection not found: {connectionKey}");
            return;
        }

        //itemSource.Database = sourceDatabase;
        //itemSource.Container = sourceContainer;

        var content = File.ReadAllText(file!);
        JObject obj = JObject.Parse(content);

        var writer = new CosmosWriter(connection);
        await writer.UpsertItemAsync(obj);
    }
}
