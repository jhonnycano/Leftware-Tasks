using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.DTO;
using Leftware.Tasks.Core.TaskParameters;
using Newtonsoft.Json;

namespace Leftware.Tasks.Impl.General.Collections;

[Descriptor("General - Import collections")]
public class ImportCollectionsTask : CommonTaskBase
{
    private const string FILE = "file";
    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        return new List<TaskParameter>
        {
            new ReadFileTaskParameter(FILE, "file"),
        };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        var file = UtilCollection.Get<string>(input, FILE);

        if (!File.Exists(file))
        {
            Console.WriteLine("File not found");
            return;
        }

        var json = File.ReadAllText(file);
        var collectionList = JsonConvert.DeserializeObject<IList<CollectionHolderDTO>>(json);

        var provider = Context.CollectionProvider;

        foreach(var col in collectionList)
        {
            var name = col.Header.Name;
            var type = col.Header.ItemType;
            var schema = col.Header.Schema == null ? default : JsonConvert.SerializeObject(col.Header.Schema);
            provider.RemoveCollection(name);

            await provider.AddCollectionAsync(name, type, schema);
            foreach(var item in col.Items)
            {
                var content = type == CollectionItemType.JsonObject ?
                    JsonConvert.SerializeObject(item.Content) :
                    item.Content.ToString() ?? "";
                await provider.AddItemAsync(name, item.Key, item.Label, content);
            }
        }
    }
}
