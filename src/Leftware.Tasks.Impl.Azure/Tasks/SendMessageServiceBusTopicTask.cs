using Azure.Messaging.ServiceBus;
using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.Model;
using Leftware.Tasks.Core.TaskParameters;
using Leftware.Tasks.Core.TaskParameters.Conditions;
using Newtonsoft.Json;

namespace Leftware.Tasks.Impl.Azure.Tasks;

[Descriptor("Azure - Send message to service bus topic")]
public class SendMessageServiceBusTopicTask : CommonTaskBase
{
    private const string CONNECTION = "connection";
    private const string SOURCE_TYPE = "source-type";
    private const string SOURCE_VALUE = "source-value";

    private class MessageSourceOptions
    {
        public const string File = "File";
        public const string ConsoleInput = "Console Input";
        public const string ContextProperty = "Context Property";
    }

    public override IList<TaskParameter> GetTaskParameterDefinition()
    {
        var sourceTypes = new[] { MessageSourceOptions.File, MessageSourceOptions.ConsoleInput, MessageSourceOptions.ContextProperty };
        return new List<TaskParameter>
        {
            new SelectFromCollectionTaskParameter(CONNECTION, "connection", Defs.Collections.AZURE_SERVICE_BUS_TOPIC_CONNECTION),
            new SelectStringTaskParameter(SOURCE_TYPE, "Content source type", sourceTypes),
            new ReadStringTaskParameter(SOURCE_VALUE, "Content source value")
                .WithSchema<ServiceBusTopicMessage>()
                .When(new EqualsCondition(SOURCE_TYPE, MessageSourceOptions.ConsoleInput)),
            new ReadFileTaskParameter(SOURCE_VALUE, "Content file")
                .WithSchema<ServiceBusTopicMessage>()
                .When(new EqualsCondition(SOURCE_TYPE, MessageSourceOptions.File)),
        };
    }

    public override async Task Execute(IDictionary<string, object> input)
    {
        var connectionKey = input.Get(CONNECTION, "");
        var sourceType = input.Get(SOURCE_TYPE, "");
        var sourceValue = input.Get(SOURCE_VALUE, "");

        var connection = Context.CollectionProvider.GetItemContentAs<ServiceBusTopicConnection>(Defs.Collections.AZURE_SERVICE_BUS_TOPIC_CONNECTION, connectionKey);
        var messageInfo = GetMessage(sourceType, sourceValue);
        if (messageInfo == null) return;

        var messageId = Guid.NewGuid().ToString();
        var timestamp = DateTime.UtcNow.AddSeconds(-10).ToString("o");
        var msg = JsonConvert.SerializeObject(messageInfo.Content);
        msg = msg
            .Replace("@@timestamp", timestamp)
            .Replace("@@guid", messageId);

        Console.WriteLine($"Sending message. Id: {messageId}, Timestamp: {timestamp}");

        var client = new ServiceBusClient(connection.Connection);
        var sender = client.CreateSender(connection.Topic);
        var message = new ServiceBusMessage(msg);
        message.SessionId = Guid.NewGuid().ToString();
        message.ContentType = "application/json";

        foreach (var prop in messageInfo.ApplicationProperties)
        {
            message.ApplicationProperties.Add(prop.Key, prop.Value);
        }

        try
        {
            var task = sender.SendMessageAsync(message);
            task.Wait();
            Console.WriteLine("Message sent!");
        }
        finally
        {
            if (sender != null) await sender.DisposeAsync();
            if (client != null) await client.DisposeAsync();
        }
    }

    private ServiceBusTopicMessage? GetMessage(string sourceType, string sourceValue)
    {
        string? jsonMessage;
        switch (sourceType)
        {
            case MessageSourceOptions.File:
                var fileContent = File.ReadAllText(sourceValue);
                jsonMessage = fileContent;
                break;
            case MessageSourceOptions.ConsoleInput:
                jsonMessage = sourceValue;
                break;
            case MessageSourceOptions.ContextProperty:
                if (!Context.ExtendedInfo.ContainsKey(sourceValue))
                {
                    UtilConsole.WriteError($"Key not found in context: {sourceValue}");
                    return null;
                }
                jsonMessage = Context.ExtendedInfo[sourceValue] as string;
                break;
            default: return null;
        }

        if (jsonMessage == null) return null;

        var message = JsonConvert.DeserializeObject<ServiceBusTopicMessage>(jsonMessage);
        return message;
    }
}
