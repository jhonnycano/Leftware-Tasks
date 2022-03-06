namespace Leftware.Tasks.Core;

public static class Defs
{
    public const string MANUAL_INPUT_LABEL = "-- Input manual value";
    public const string USE_AS_VALUE = "-- Use as value --";
    public const string CANCEL_LABEL = "-- Return to main menu";
    public const string VALUE_NOT_FOUND = "--Value not found--";
    public const string DEFAULT_CANCEL_STRING = "?";

    public static class Collections
    {
        public const string SETTINGS = "settings";

        public const string FAVORITE_FILE = "favorite-file";
        public const string FAVORITE_FOLDER = "favorite-folder";
        public const string FILE_PATTERN = "file-pattern";
        public const string SYMLINK_SOURCE = "symlink-source";
        public const string SYMLINK_TARGET = "symlink-target";
        public const string XML_NS_FILE = "xml-ns-file";
        
        public const string AZURE_COSMOS_CONNECTION = "az-cosmos-connection";
        public const string AZURE_COSMOS_DATABASE = "az-cosmos-database";
        public const string AZURE_COSMOS_CONTAINER = "az-cosmos-container";
        public const string AZURE_SERVICE_BUS_TOPIC_CONNECTION = "az-sb-topic-connection";
    }

    public static class Settings
    {
        public const string PATH_7ZIP = "path-7zip";
    }
}
