using Newtonsoft.Json.Linq;

namespace Leftware.Tasks.Core.Model;

public class ServiceBusTopicMessage
{
    public JToken Content { get; set; }
    public IDictionary<string, string> ApplicationProperties { get; set; }

    public ServiceBusTopicMessage()
    {
        Content = JToken.Parse("{}");
        ApplicationProperties = new Dictionary<string, string>();
    }

    public ServiceBusTopicMessage(JToken content, IDictionary<string, string> applicationProperties)
    {
        Content = content;
        ApplicationProperties = applicationProperties;
    }
}
