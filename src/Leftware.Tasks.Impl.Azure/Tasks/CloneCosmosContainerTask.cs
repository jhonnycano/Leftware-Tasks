using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.Model;
using Spectre.Console;

namespace Leftware.Tasks.Impl.Azure.Tasks;

[Descriptor("Azure - Clone Cosmos container")]
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

        var items = _collectionProvider.GetItems("cosmos-connection");
        var connectionSource = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .Title("[green]Cosmos Connection source[/]")
            .AddChoices(items.Select(i => i.Label))
            );
        var sourceConnectionItem = items.First(i => i.Label == connectionSource);
        var sourceConnection = sourceConnectionItem.As<CosmosConnection>() ?? throw new InvalidOperationException("Could not convert value");
        dic["source-connection"] = sourceConnectionItem.Key;

        if (!GetString(dic, "source-database", "Source Database", sourceConnection.Database, "cosmos-database")) return null;
        if (!GetString(dic, "source-container", "Source Container", sourceConnection.Container, "cosmos-container")) return null;

        var connectionTarget = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .Title("[green]Cosmos Connection target[/]")
            .AddChoices(items.Select(i => i.Label))
            );
        var targetConnectionItem = items.First(i => i.Label == connectionTarget);
        var targetConnection = sourceConnectionItem.As<CosmosConnection>() ?? throw new InvalidOperationException("Could not convert value");
        dic["target-connection"] = targetConnectionItem.Key;

        if (!GetString(dic, "target-database", "Target Database", targetConnection.Database, "cosmos-database")) return null;
        if (!GetString(dic, "target-container", "Target Container", targetConnection.Container, "cosmos-container")) return null;

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
        await writer.WriteElements(list);
    }
}
