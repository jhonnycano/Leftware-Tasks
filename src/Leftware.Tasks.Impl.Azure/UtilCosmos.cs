using Microsoft.Azure.Cosmos;

namespace CosmosCloner;

public static class UtilCosmos
{
    public static CosmosClient? GetCosmosClient(string endpointUri, string key)
    {
        try
        {
            var cosmosClient = new CosmosClient(endpointUri, key);
            Console.WriteLine("CosmosClient loaded successfully");
            return cosmosClient;
        }
        catch (CosmosException de)
        {
            Exception baseException = de.GetBaseException();
            Console.WriteLine("{0} error occurred: {1}", de.StatusCode, de);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: {0}", e);
        }
        return null;
    }

    public static async Task<Database> LoadDatabaseAsync(CosmosClient client, string databaseId)
    {
        // To create a new database:
        // await client.CreateDatabaseIfNotExistsAsync(databaseId);

        var result = client.GetDatabase(databaseId);
        Console.WriteLine("Loaded Database: {0}\n", result.Id);
        return await Task.FromResult(result);
    }

    public static async Task<Container> LoadContainerAsync(Database database, string containerId)
    {
        // Create a new container
        /*
        var result = await database.CreateContainerIfNotExistsAsync(containerId, "/Id");
        Console.WriteLine("Loaded Container: {0}\n", result.Container.Id);
        return result.Container;
        */
        var container = database.GetContainer(containerId);
        return await Task.FromResult(container);
    }
}
