namespace Leftware.Tasks.Core.Model
{
    public class CosmosConnection
    {
        public string EndpointUrl { get; set; }
        public string AccessKey { get; set; }
        public string? Database { get; set; }
        public string? Container { get; set; }

        public CosmosConnection()
        {
            EndpointUrl = "";
            AccessKey = "";
            Database = null;
            Container = null;
        }

        public CosmosConnection(string endpointUrl, string accessKey, string? database = null, string? container = null)
        {
            EndpointUrl = endpointUrl;
            AccessKey = accessKey;
            Database = database;
            Container = container;
        }
    }
}
