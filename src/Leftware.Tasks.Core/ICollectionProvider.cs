﻿using Leftware.Tasks.Core.Model;

namespace Leftware.Tasks.Core;

public enum CollectionItemType
{
    None, 
    String, 
    File, 
    Folder, 
    JsonObject, 
}

public interface IShouldExist
{
    public bool ShouldExist { get; set; }
}

public interface ICollectionProvider
{
    IList<string> GetCollections();
    CollectionHeader? GetHeader(string col);
    IList<CollectionItem> GetItems(string collection);
    CollectionItem GetItem(string collection, string key);
    T GetItemContentAs<T>(string collection, string key);

    Task AddCollectionAsync(string name, CollectionItemType type, string? schema = null);
    Task AddItemAsync(string collection, string key, string label, string content);

    void UpdateItemLabel(string collection, string key, string label);
    void UpdateItemContent(string collection, string key, string content);

    void RemoveCollection(string name);

    void RemoveItem(string collection, string key);
}