using Leftware.Injection.Attributes;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.Model;
using Leftware.Tasks.Persistence.Resources;

namespace Leftware.Tasks.Persistence;

[Service]
public class DatabaseInitializer
{
    private readonly SqliteDatabaseProvider _dbProvider;
    private readonly ICollectionProvider _collectionProvider;
    private readonly ISettingsProvider _settingsProvider;

    public DatabaseInitializer(
        SqliteDatabaseProvider databaseProvider,
        ICollectionProvider collectionProvider,
        ISettingsProvider settingsProvider
        )
    {
        _dbProvider = databaseProvider;
        _collectionProvider = collectionProvider;
        _settingsProvider = settingsProvider;
    }

    public async Task ValidateAsync()
    {
        if (File.Exists(_dbProvider.FilePath))
        {
            if (CheckDatabase()) return;
            File.Delete(_dbProvider.FilePath);
            // Thread.Sleep(500);
        }

        await CreateDatabaseAsync();
    }

    public bool CheckDatabase()
    {
        var sql = "SELECT name FROM sqlite_master WHERE type='table' AND name='col_header';";
        var list = _dbProvider.GetList(sql);
        return list.Count > 0;
    }

    private async Task CreateDatabaseAsync()
    {
        var sql = FileResources.Db_Create;
        _dbProvider.Execute(sql, null);

        await _collectionProvider.AddCollectionAsync(Defs.Collections.FAVORITE_FILE, CollectionItemType.File);
        await _collectionProvider.AddCollectionAsync(Defs.Collections.FAVORITE_FOLDER, CollectionItemType.Folder);
        
        var cosmosConnectionSchema = UtilJsonSchema.GetJsonSchemaForType<CosmosConnection>();
        await _collectionProvider.AddCollectionAsync(Defs.Collections.AZURE_COSMOS_CONNECTION, CollectionItemType.JsonObject, cosmosConnectionSchema);
        await _collectionProvider.AddCollectionAsync(Defs.Collections.AZURE_COSMOS_DATABASE, CollectionItemType.String);
        await _collectionProvider.AddCollectionAsync(Defs.Collections.AZURE_COSMOS_CONTAINER, CollectionItemType.String);

        var serviceBusTopicConnectionSchema = UtilJsonSchema.GetJsonSchemaForType<ServiceBusTopicConnection>();
        await _collectionProvider.AddCollectionAsync(Defs.Collections.AZURE_SERVICE_BUS_TOPIC_CONNECTION, CollectionItemType.JsonObject, serviceBusTopicConnectionSchema);


        //_settingsProvider.SetSetting(D)
    }
}
