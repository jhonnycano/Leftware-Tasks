using CosmosCloner;
using Leftware.Tasks.Core.Model;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;

namespace Leftware.Tasks.Impl.Azure;

public class CosmosWriter
{
    private readonly CosmosConnection _cn;

    public CosmosWriter(
        CosmosConnection cn
        )
    {
        _cn = cn;
    }

    public async Task WriteElementsAsync(IList<dynamic> list)
    {
        var container = await GetContainerAsync();
        if (container == null) return;

        await WriteElementsInnerAsync(container, list);
    }

    public async Task UpsertItemAsync(JObject obj)
    {
        var container = await GetContainerAsync();
        if (container == null) return;

        var task = container.UpsertItemAsync<dynamic>(obj);
        task.Wait();
    }

    private async Task<Container?> GetContainerAsync()
    {
        var cosmosClient = UtilCosmos.GetCosmosClient(_cn.EndpointUrl, _cn.AccessKey);
        if (cosmosClient == null) return null;

        var database = await UtilCosmos.LoadDatabaseAsync(cosmosClient, _cn.Database);
        var container = await UtilCosmos.LoadContainerAsync(database, _cn.Container);
        return container;
    }

    private async Task WriteElementsInnerAsync(Container container, IList<dynamic> list)
    {
        try
        {
            foreach (var item in list)
            {
                //var itemKey = item["CustomerId"].ToString();
                //var partitionKey = new PartitionKey(itemKey);
                var task = container.UpsertItemAsync<dynamic>(item);
                task.Wait();
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine("{0} occurred: {1}, {2}", ex.GetType().Name, ex.Message, ex.StackTrace);
        }
    }

}
