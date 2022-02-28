using Leftware.Injection.Attributes;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.Model;
using Leftware.Tasks.Persistence.Resources;
using NJsonSchema;

namespace Leftware.Tasks.Persistence
{
    [Service]
    public class CollectionInitializer
    {
        private readonly SqliteDatabaseProvider _dbProvider;
        private readonly ICollectionProvider _collectionProvider;

        public CollectionInitializer(
            SqliteDatabaseProvider databaseProvider,
            ICollectionProvider collectionProvider
            )
        {
            _dbProvider = databaseProvider;
            _collectionProvider = collectionProvider;
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

            var cosmosConnectionSchema = GetJsonSchemaForType<CosmosConnection>();
            await _collectionProvider.AddCollectionAsync("cosmos-connection", CollectionItemType.JsonObject, cosmosConnectionSchema);
            await _collectionProvider.AddCollectionAsync("cosmos-database", CollectionItemType.String);
            await _collectionProvider.AddCollectionAsync("cosmos-container", CollectionItemType.String);
        }

        private static string GetJsonSchemaForType<T>()
        {
            var schema = JsonSchema.FromType<T>();
            var schemaData = schema.ToJson();
            return schemaData;
        }
    }
}
