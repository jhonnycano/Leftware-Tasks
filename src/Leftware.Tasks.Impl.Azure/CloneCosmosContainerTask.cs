using Leftware.Common;
using Leftware.Tasks.Core;
using Spectre.Console;

namespace Leftware.Tasks.Impl.Azure;

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
        var connection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .Title("[green]Cosmos Connection[/]")
            .AddChoices(items.Select(i => i.Label))
            );
        dic["connection-source"] = items.First(i => i.Label == connection).Key;
        return dic;
    }
    public override async Task Execute(IDictionary<string, object> input)
    {
        var connectionSource = UtilCollection.Get(input, "connection-source", "");

        var items = _collectionProvider.GetItems("cosmos-connection");
        var item = items.FirstOrDefault(i => i.Key == connectionSource);
        if (item == null)
        {
            UtilConsole.WriteError("Source connection not found");
            return;
        }

    }
}
