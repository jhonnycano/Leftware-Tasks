using CosmosCloner;
using Leftware.Tasks.Core.Model;
using Microsoft.Azure.Cosmos;

namespace Leftware.Tasks.Impl.Azure;

public class CosmosReader
{
    private readonly CosmosConnection _cn;

    public CosmosReader(
        CosmosConnection cn
        )
    {
        _cn = cn;
    }

    public async Task<IList<dynamic>?> ReadElementsFromQuery(string sql)
    {
        var cosmosClient = UtilCosmos.GetCosmosClient(_cn.EndpointUrl, _cn.AccessKey);
        if (cosmosClient == null) return null;

        var database = await UtilCosmos.LoadDatabaseAsync(cosmosClient, _cn.Database!);
        var container = await UtilCosmos.LoadContainerAsync(database, _cn.Container!);

        var list = await ReadElements(container, sql);
        return list;
    }

    private async Task<IList<dynamic>> ReadElements(Container container, string sql)
    {
        var qd = new QueryDefinition(sql)
            //.WithParameter("@customerId", customerId)
            ;
        var resultSet = container.GetItemQueryIterator<dynamic>(qd);

        var resultList = new List<dynamic>();
        while (resultSet.HasMoreResults)
        {
            var task = resultSet.ReadNextAsync();
            task.Wait();
            var item = task.Result;
            foreach (var obj in item)
            {
                Console.WriteLine("\tRead {0}\n", obj);
                resultList.Add(obj);
            }
        }
        return resultList;
    }
}
