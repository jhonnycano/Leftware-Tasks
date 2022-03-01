namespace Leftware.Tasks.Core.Model;

public class ServiceBusTopicConnection
{
    public string Connection { get; set; }
    public string Topic { get; set; }
    public string Subscription { get; set; }

    public ServiceBusTopicConnection()
    {
        Connection = "";
        Topic = "";
        Subscription = "";
    }

    public ServiceBusTopicConnection(string connection, string topic, string subscription)
    {
        Connection = connection;
        Topic = topic;
        Subscription = subscription;
    }
}
