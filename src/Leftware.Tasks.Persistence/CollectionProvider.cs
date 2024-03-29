﻿using Leftware.Injection.Attributes;
using Leftware.Tasks.Core.Model;
using Leftware.Tasks.Persistence;
using Newtonsoft.Json.Linq;
using NJsonSchema;

namespace Leftware.Tasks.Core;

[InterfaceImplementationDefault]
public class CollectionProvider : ICollectionProvider
{
    private readonly SqliteDatabaseProvider _provider;

    public CollectionProvider(
        SqliteDatabaseProvider provider
        )
    {
        _provider = provider;
    }

    public IList<string> GetCollections()
    {
        var sql = "SELECT name FROM col_header";
        var list = _provider.GetList(sql);
        return list;
    }

    public CollectionHeader? GetHeader(string col)
    {
        var sql = "SELECT * FROM col_header WHERE name = @name";
        var result = _provider.GetObject<CollectionHeader>(sql, new { name = col });
        return result;
    }

    public IList<CollectionItem> GetItems(string collection)
    {
        var sql = "SELECT * FROM col_item WHERE col = @collection";
        var items = _provider.GetObjects<CollectionItem>(sql, new { collection });
        return items;
    }

    public CollectionItem GetItem(string collection, string key)
    {
        var items = GetItems(collection);
        if (items == null || items.Count == 0) throw new InvalidOperationException($"Collection empty. {collection}");

        var item = items.FirstOrDefault(i => i.Key == key);
        if (item == null) throw new InvalidOperationException($"Key not found in collection. {collection}.{key}");

        return item;
    }

    public T GetItemContentAs<T>(string collection, string key)
    {
        var item = GetItem(collection, key);
        return item.As<T>();
    }

    public async Task AddCollectionAsync(string name, CollectionItemType itemType, string? schema = null)
    {
        if (itemType == CollectionItemType.JsonObject && schema == null) throw new ArgumentNullException(nameof(schema));

        if (schema != null)
        {
            var schemaObject = await JsonSchema.FromJsonAsync(schema);
            // if (schemaObject.is)
        }

        string sql = "INSERT INTO col_header (name, itemType, schema) VALUES (@name, @itemType, @schema);";
        _provider.Execute(sql, new { name, itemType, schema });
        // todo: return col id ?
    }

    public async Task AddItemAsync(string collection, string key, string label, string content)
    {
        // todo: validate collection exists and get schema
        var sql = "SELECT * FROM col_header WHERE name = @name";
        var col = _provider.GetObject<CollectionHeader>(sql, new { name = collection })
            ?? throw new InvalidOperationException($"Collection not found. {collection}");

        // todo: validate content against col schema
        if (col.Schema != null)
        {
            var schema = await JsonSchema.FromJsonAsync(col.Schema);
            var token = JToken.Parse(content);
            var validationErrors = schema.Validate(token);
            if (validationErrors != null && validationErrors.Count > 0)
            {
                //validationErrors.First().
                return;
            }
        }

        sql = "INSERT INTO col_item (col, key, label, content) VALUES (@col, @key, @label, @content);";
        _provider.Execute(sql, new { col = collection, key, label, content });
    }

    public void RemoveCollection(string name)
    {
        var par = new { col = name };

        var sql = "DELETE FROM col_item WHERE col = @col";
        _provider.Execute(sql, par);

        sql = "DELETE FROM col_header WHERE name = @col";
        _provider.Execute(sql, par);
    }

    public void RemoveItem(string collection, string key)
    {
        var par = new { col = collection, key };

        var sql = "DELETE FROM col_item WHERE col = @col AND key = @key";
        _provider.Execute(sql, par);
    }

    public void UpdateItemContent(string collection, string key, string content)
    {
        throw new NotImplementedException();
    }

    public void UpdateItemLabel(string collection, string key, string label)
    {
        throw new NotImplementedException();
    }
}