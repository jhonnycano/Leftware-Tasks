using CosmosCloner;
using Leftware.Tasks.Core.Model;
using Microsoft.Azure.Cosmos;

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

    public async Task WriteElements(IList<dynamic> list)
    {
        var cosmosClient = UtilCosmos.GetCosmosClient(_cn.EndpointUrl, _cn.AccessKey);
        if (cosmosClient == null) return;

        var database = await UtilCosmos.LoadDatabaseAsync(cosmosClient, _cn.Database);
        var container = await UtilCosmos.LoadContainerAsync(database, _cn.Container);

        await WriteElementsInner(container, list);
    }

    private async Task WriteElementsInner(Container container, IList<dynamic> list)
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
