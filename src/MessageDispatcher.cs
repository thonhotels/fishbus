using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;

namespace Thon.Hotels.FishBus
{
    public class MessageDispatcher
    {
        private IReceiverClient Client { get; }

        private IServiceScopeFactory ScopeFactory { get; }

        public Func<Microsoft.Azure.ServiceBus.Message, Task> CompleteAsyncDelegate { get; set; }
        private MessageHandlerRegistry Registry { get; }

        public MessageDispatcher(IServiceScopeFactory scopeFactory, IReceiverClient client, MessageHandlerRegistry registry)
        {
            ScopeFactory = scopeFactory;
            Client = client;
            Registry = registry;
        }

        // Call all handlers for the message type given by the message label.
        // There can be multiple handlers per message type
        public async Task ProcessMessage(Microsoft.Azure.ServiceBus.Message message, CancellationToken token)
        {
            var body = Encoding.UTF8.GetString(message.Body);
            Log.Debug($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{body}");
            try
            {
                if (!string.IsNullOrWhiteSpace(message.Label))
                {
                    await ProcessMessage(message.Label, body, () => Client.CompleteAsync(message.SystemProperties.LockToken));
                }
                else
                {
                    Log.Error("Message label is not set. \n Message: {@messageBody} \n Forwarding to DLX", body);
                    await AddToDeadLetter(message.SystemProperties.LockToken, "Message label is not set.");
                }
            }
            catch (JsonException jsonException)
            {
                Log.Error(jsonException, "Unable to deserialize message. \n Message: {@messageBody} \n Forwarding to DLX", body);
                await AddToDeadLetter(message.SystemProperties.LockToken, jsonException.Message);
            }
        }

        private async Task ProcessMessage(string label, string body, Func<Task> markCompleted)
        {
            var typeFromLabel = TypeFromLabel(label);
            if (typeFromLabel != default(Type))
            {
                var command = JsonConvert.DeserializeObject(body, typeFromLabel);

                using (var scope = ScopeFactory.CreateScope())
                {
                    foreach (var handler in Registry.GetHandlers(command.GetType(), scope))
                        await CallHandler(handler, command, markCompleted);
                }
            }
            else
            {
                Log.Debug("", body);
            }
        }

        private System.Type TypeFromLabel(string label)
        {
            return typeof(MessageDispatcher).Assembly.GetTypes().FirstOrDefault(type => type.FullName == label);
        }

        private Task CallHandler(object handler, object message, Func<Task> markCompleted)
        {
            var tasks =
                handler
                    .GetType()
                    .GetMethods()
                    .Where(m => m.Name == "Handle")
                    .Select(m => (Task)m.Invoke(handler, new[] { message, markCompleted }))
                    .ToArray();

            Task.WaitAll(tasks);
            return Task.CompletedTask;
        }

        internal async Task Close()
        {
            await Client.CloseAsync();
        }

        internal void RegisterMessageHandler(Func<ExceptionReceivedEventArgs, Task> exceptionReceivedHandler)
        {
            Client.RegisterMessageHandler(ProcessMessage, new MessageHandlerOptions(exceptionReceivedHandler)
            {
                AutoComplete = false,
            });
        }

        private async Task AddToDeadLetter(string lockToken, string errorMessage)
        {
            await Client.DeadLetterAsync(lockToken, "Invalid message", errorMessage);
        }
    }
}
