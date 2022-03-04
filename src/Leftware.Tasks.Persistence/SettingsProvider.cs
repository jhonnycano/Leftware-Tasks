using Leftware.Common;
using Leftware.Injection.Attributes;

namespace Leftware.Tasks.Core;

[InterfaceImplementationDefault]
public class SettingsProvider : ISettingsProvider
{
    private readonly ICollectionProvider _collectionProvider;

    public SettingsProvider(
        ICollectionProvider collectionProvider
        )
    {
        _collectionProvider = collectionProvider;
    }
    public string? GetSetting(string name, bool askUserIfNotFound = false)
    {
        var value = GetValue(Defs.Collections.SETTINGS, name);
        if (value != Defs.VALUE_NOT_FOUND) return value;
        if (!askUserIfNotFound) return null;

        var innerValue = UtilConsole.ReadString(string.Format("Setting {0} not found. Setup {0}:", name));
        if (innerValue == "") return null;

        SetSetting(name, innerValue);
        return value;
    }

    public void SetSetting(string name, string value)
    {
        _collectionProvider.AddItemAsync(Defs.Collections.SETTINGS, name, name, value);
    }

    private string GetValue(string collection, string name)
    {
        var list = _collectionProvider.GetItems(collection);
        var config = list.FirstOrDefault(cv => cv.Key == name);
        if (config != null) return config.Content;
        return Defs.VALUE_NOT_FOUND;
    }
}