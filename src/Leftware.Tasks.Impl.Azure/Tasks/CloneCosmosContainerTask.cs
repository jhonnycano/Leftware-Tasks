using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.Model;

namespace Leftware.Tasks.Impl.Azure.Tasks;

[Descriptor("Azure Cosmos - Clone container")]
public class CloneCosmosContainerTask : CommonTaskBase
{
    private readonly ICollectionProvider _collectionProvider;

    public CloneCosmosContainerTask(
        ICollectionProvider collectionProvider
        )
    {
        _collectionProvider = collectionProvider;
    }
    public override async Task<IDictionary<string, object>?> GetTaskInput()
    {
        var dic = GetEmptyTaskInput();

        var sourceConnectionItem = Input.GetItem(dic, "source-connection", "Connection to CosmosDB", "cosmos-connection");
        if (sourceConnectionItem == null) return null;

        var sourceConnection = sourceConnectionItem.As<CosmosConnection>();

        if (!Input.GetStringFromCollection(dic, "source-database", "Source Database", "cosmos-database", sourceConnection.Database)) return null;
        if (!Input.GetStringFromCollection(dic, "source-container", "Source Container", "cosmos-container", sourceConnection.Container)) return null;

        var targetConnectionItem = Input.GetItem(dic, "target-connection", "Connection to CosmosDB", "cosmos-connection");
        if (targetConnectionItem == null) return null;

        var targetConnection = targetConnectionItem.As<CosmosConnection>();

        if (!Input.GetStringFromCollection(dic, "target-database", "Target Database", "cosmos-database", targetConnection.Database)) return null;
        if (!Input.GetStringFromCollection(dic, "target-container", "Target Container", "cosmos-container", targetConnection.Container)) return null;

        return dic;
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        var sourceConnectionKey = input.Get("source-connection", "");
        var sourceDatabase = input.Get("source-database", "");
        var sourceContainer = input.Get("source-container", "");
        var connectionTargetKey = input.Get("target-connection", "");
        var targetDatabase = input.Get("target-database", "");
        var targetContainer = input.Get("target-container", "");

        var itemSource = _collectionProvider.GetItemContentAs<CosmosConnection>("cosmos-connection", sourceConnectionKey!);
        if (itemSource == null)
        {
            UtilConsole.WriteError($"Source connection not found: {sourceConnectionKey}");
            return;
        }
        var itemTarget = _collectionProvider.GetItemContentAs<CosmosConnection>("cosmos-connection", connectionTargetKey!);
        if (itemTarget == null)
        {
            UtilConsole.WriteError($"Target connection not found: {connectionTargetKey}");
            return;
        }

        itemSource.Database = sourceDatabase;
        itemSource.Container = sourceContainer;
        itemTarget.Database = targetDatabase;
        itemTarget.Container = targetContainer;

        string sql = "SELECT * FROM c";

        var reader = new CosmosReader(itemSource);
        var list = await reader.ReadElementsFromQuery(sql);
        if (list == null)
        {
            UtilConsole.WriteError("Could not find input on source");
            return;
        }

        var writer = new CosmosWriter(itemTarget);
        await writer.WriteElementsAsync(list);
    }
}
