namespace Leftware.Tasks.Core;

public interface ISettingsProvider
{
    string? GetSetting(string name, bool askUserIfNotFound = false);

    void SetSetting(string name, string value);
}
