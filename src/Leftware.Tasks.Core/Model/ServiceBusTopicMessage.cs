namespace Leftware.Tasks.Core.Model;

public class ServiceBusTopicMessage
{
    public object Content { get; set; }
    public IDictionary<string, string> ApplicationProperties { get; set; }

    public ServiceBusTopicMessage()
    {
        Content = new { };
        ApplicationProperties = new Dictionary<string, string>();
    }

    public ServiceBusTopicMessage(object content, IDictionary<string, string> applicationProperties)
    {
        Content = content;
        ApplicationProperties = applicationProperties;
    }
}
