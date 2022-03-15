using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.DTO;
using Leftware.Tasks.Core.Model;
using Leftware.Tasks.Core.TaskParameters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using Spectre.Console;

namespace Leftware.Tasks.Impl.General.Collections;

[Descriptor("General - Export collections")]
public class ExportCollectionsTask : CommonTaskBase
{
    private const string FILE = "file";
    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        return new List<TaskParameter>
        {
            new ReadFileTaskParameter(FILE, "file", false),
        };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        var file = UtilCollection.Get<string>(input, FILE);

        var provider = Context.CollectionProvider;
        var cols = provider.GetCollections();
        var list = new List<CollectionHolderDTO>();
        foreach(var col in cols)
        {
            var header = provider.GetHeader(col);
            var items = provider.GetItems(col);
            var collection = new CollectionHolderDTO();
            collection.Header = new CollectionHeaderDTO(header);
            collection.Items = items.Select(i => new CollectionItemDTO(header, i)).ToList();
            list.Add(collection);
        }

        var json = JsonConvert.SerializeObject(list);
        File.WriteAllText(file, json);
    }
}
