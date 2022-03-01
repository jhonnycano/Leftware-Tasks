using Azure.Messaging.ServiceBus;
using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.Model;
using Newtonsoft.Json;

namespace Leftware.Tasks.Impl.Azure.Tasks
{
    [Descriptor("Azure - Send message to service bus topic")]
    public class SendMessageServiceBusTopicTask : CommonTaskBase
    {
        private class MessageSourceOptions
        {
            public const string File = "File";
            public const string ConsoleInput = "Console Input";
            public const string ContextProperty = "Context Property";
        }

        public override async Task<IDictionary<string, object>?> GetTaskInput()
        {
            var dic = GetEmptyTaskInput();

            var connectionItem = Input.GetItem(dic, "connection", "Connection to Service Bus Topic", "service-bus-topic-connection");
            if (connectionItem == null) return null;
            _ = connectionItem.As<ServiceBusTopicConnection>();

            var messageSourceOptions = new[] { MessageSourceOptions.File, MessageSourceOptions.ConsoleInput, MessageSourceOptions.ContextProperty };
            var source = Input.SelectOption(dic, "source-type", "Type of source for the message", messageSourceOptions);
            if (source == null) return null;

            switch (source)
            {
                case MessageSourceOptions.File:
                    if (!Input.GetExistingFile(dic, "source-value", "File path")) return null;
                    break;
                case MessageSourceOptions.ConsoleInput:
                    var messageSchema = UtilJsonSchema.GetJsonSchemaForType<ServiceBusTopicMessage>();
                    if (!Input.GetJson(dic, "source-value", "Json message", null, messageSchema)) return null;
                    break;
                case MessageSourceOptions.ContextProperty:
                    if (!Input.GetString(dic, "source-value", "Context property key")) return null;
                    break;
            }
            return dic;
        }

        public override async Task Execute(IDictionary<string, object> input)
        {
            var connectionKey = input.Get("connection", "");
            var sourceType = input.Get("source-type", "");
            var sourceValue = input.Get("source-value", "");

            var connection = Context.CollectionProvider.GetItemContentAs<ServiceBusTopicConnection>("service-bus-topic-connection", connectionKey);
            var messageInfo = GetMessage(sourceType, sourceValue);
            if (messageInfo == null) return;

            var messageId = Guid.NewGuid().ToString();
            var timestamp = DateTime.UtcNow.ToString("o");
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
}
