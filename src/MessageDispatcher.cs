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

// tests need access to this method:
// internal async Task ProcessMessage(string label, string body, Func<Task> markCompleted)
[assembly:System.Runtime.CompilerServices.InternalsVisibleTo("FishbusTests")]

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

        internal async Task ProcessMessage(string label, string body, Func<Task> markCompleted)
        {
            var typeFromLabel = Registry.GetMessageTypeByName(label);
            if (typeFromLabel != default(Type))
            {
                var message = JsonConvert.DeserializeObject(body, typeFromLabel);
            
                using (var scope = ScopeFactory.CreateScope())
                {
                    var tasks = Registry
                                    .GetHandlers(message.GetType(), scope)
                                    .ToList() // avoid deferred execution, we want all handlers to execute
                                    .Select(h => CallHandler(h, message))
                                    .ToArray();
                    var results = await Task.WhenAll(tasks);            
                    if (results.All(r => r))
                        await markCompleted();                    
                }
            }
            else
            {
                Log.Debug("No handler registered for the given {Label}. {@Body}", label, body);
                await markCompleted();
            }
        }

        private async Task<bool> CallHandler(object handler, object message)
        {
            var tasks =
                handler
                    .GetType()
                    .GetMethods()
                    .Where(m => m.Name == "Handle")
                    .Select(m => (Task<bool>)m.Invoke(handler, new[] { message }))
                    .ToArray();
            if (!tasks.Any())
                return true;
            
            if (tasks.Count() > 1)
                Log.Warning($"More than one method named Handle in type: {handler.GetType().FullName}");

            var result = await Task.WhenAll(tasks);
            return result.First();
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
