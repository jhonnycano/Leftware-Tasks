using Azure.Messaging.ServiceBus;
using Leftware.Common;
using Leftware.Tasks.Core;
using Leftware.Tasks.Core.Model;
using Newtonsoft.Json;

namespace Leftware.Tasks.Impl.Azure.Tasks
{
    [Descriptor("Azure - Receive message from service bus topic")]
    public class ReceiveMessageServiceBusTopicTask : CommonTaskBase
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

            if (!Input.GetInt(dic, "duration", "Receive Timeout (seconds)", 5, 60, 60)) return null;

            return dic;
        }

        public override async Task Execute(IDictionary<string, object> input)
        {
            var connectionKey = input.Get("connection", "");
            var duration = input.Get("duration", 0);

            var connection = Context.CollectionProvider.GetItemContentAs<ServiceBusTopicConnection>("service-bus-topic-connection", connectionKey);
            //var messageInfo = GetMessage(sourceType, sourceValue);
            //if (messageInfo == null) return;
            
            var client = new ServiceBusClient(connection.Connection);

            // create a processor that we can use to process the messages
            var processor = client.CreateSessionProcessor(connection.Topic, connection.Subscription,
                new ServiceBusSessionProcessorOptions());

            try
            {
                processor.ProcessMessageAsync += MessageHandler;
                processor.ProcessErrorAsync += ErrorHandler;
                await processor.StartProcessingAsync();

                Console.WriteLine("Wait for a minute and then press any key to end the processing");
                Thread.Sleep(duration * 1000);

                // stop processing 
                Console.WriteLine("Stopping the receiver...");
                await processor.StopProcessingAsync();
                Console.WriteLine("Stopped receiving messages");
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await processor.DisposeAsync();
                await client.DisposeAsync();
            }
        }

        static async Task MessageHandler(ProcessSessionMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            //Console.WriteLine($"Received: {body} from subscription: {subscriptionName}");

            // complete the message. messages is deleted from the subscription. 
            await args.CompleteMessageAsync(args.Message);
        }

        static Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
    }
}
