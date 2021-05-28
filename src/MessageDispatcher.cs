using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;

namespace Thon.Hotels.FishBus
{
    public class MessageDispatcher
    {
        private LogCorrelationHandler LogCorrelationHandler { get; }

        private ServiceBusClient Client { get; }

        private ServiceBusProcessor Processor { get; }

        private IServiceScopeFactory ScopeFactory { get; }

        private MessageHandlerRegistry Registry { get; }

        internal MessageDispatcher(IServiceScopeFactory scopeFactory, (ServiceBusClient, ServiceBusProcessor) c_p, MessageHandlerRegistry registry, LogCorrelationHandler logCorrelationHandler)
        {
            LogCorrelationHandler = logCorrelationHandler;
            ScopeFactory = scopeFactory;
            Client = c_p.Item1;
            Processor = c_p.Item2;
            Registry = registry;
        }

        // Call all handlers for the message type given by the message label.
        // There can be multiple handlers per message type
        public async Task ProcessMessage(ProcessMessageEventArgs args)
        {
            using (LogCorrelationHandler.PushToLogContext.Invoke(args.Message))
            {
                var body = args.Message.Body.ToString();
                var message = args.Message;
                var sw = Stopwatch.StartNew();
                Log.Debug($"Received message: SequenceNumber:{message.SequenceNumber} Body:{body}");
                try
                {

                    if (!string.IsNullOrWhiteSpace(message.Subject))
                    {
                        await ProcessMessage(message.Subject, body,
                            () => args.CompleteMessageAsync(args.Message),
                            m => AddToDeadLetter(args, m));
                    }
                    else
                    {
                        Log.Error("Message label is not set. \n Message: {@messageBody} \n Forwarding to DLX", body);
                        await AddToDeadLetter(args, "Message label is not set.");
                    }
                }
                catch (JsonException jsonException)
                {
                    Log.Error(jsonException,
                        "Unable to deserialize message. \n Message: {@messageBody} \n Forwarding to DLX", body);
                    await AddToDeadLetter(args, jsonException.Message);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Caught exception while handling message with label {Label} and body {Body}",
                        message.Subject, body);
                    throw;
                }

                Log.Debug($"Completed handling message in {sw.ElapsedMilliseconds} ms");
            }
        }

        internal async Task ProcessMessage(string subject, string body, Func<Task> markCompleted, Func<string, Task> abort)
        {
            var typeFromSubject = Registry.GetMessageTypeByName(subject);
            if (typeFromSubject != default)
            {
                var message = JsonConvert.DeserializeObject(body, typeFromSubject);

                var scopeAndHandlers = Registry.GetHandlers(ScopeFactory, message.GetType());
                var tasks = scopeAndHandlers
                    .ToList() // avoid deferred execution, we want all handlers to execute
                    .Select(h => CallHandler(h.handler, message, abort))
                    .ToArray();
                var results = await Task.WhenAll(tasks);
                if (results.All(r => r))
                    await markCompleted();
                foreach (var scope in scopeAndHandlers.Select(s => s.scope))
                {
                    scope.Dispose();
                }
            }
            else
            {
                Log.Debug("No handler registered for the given {Label}. {@Body}", subject, body);
                await markCompleted();
            }
        }

        private async Task<bool> CallHandler(object handler, object message, Func<string, Task> abort)
        {
            var tasks =
                handler
                    .GetType()
                    .GetMethods()
                    .Where(m => HandlesMessageOfType(m, message.GetType()))
                    .Select(m => (Task<HandlerResult>)m.Invoke(handler, new[] { message }))
                    .ToArray();
            if (!tasks.Any())
                return true;

            if (tasks.Count() > 1)
                Log.Warning($"More than one method named Handle in type: {handler.GetType().FullName}");

            var results = await Task.WhenAll(tasks);
            var aborted = results.FirstOrDefault(HandlerResult.IsAbort);
            if (aborted != null)
            {
                await abort(aborted.Message);
                return false;
            }
            return !results.Any(HandlerResult.IsFailed);
        }

        private bool HandlesMessageOfType(MethodInfo m, Type type)
        {
            var parameters = m.GetParameters();
            if (m.Name != "Handle" || !parameters.Any()) return false;
            return parameters[0].ParameterType == type;
        }

        internal async Task Close()
        {
            await Processor.StopProcessingAsync();
            await Processor.CloseAsync();
            await Client.DisposeAsync();
        }

        internal Task RegisterMessageHandler(Func<ProcessErrorEventArgs, Task> exceptionReceivedHandler)
        {
            Processor.ProcessMessageAsync += ProcessMessage;
            Processor.ProcessErrorAsync += exceptionReceivedHandler;
            return Processor.StartProcessingAsync();
            // Processor.RegisterMessageHandler(ProcessMessage, new MessageHandlerOptions(exceptionReceivedHandler)
            // {
            //     AutoComplete = false,
            // });
        }

        private Task AddToDeadLetter(ProcessMessageEventArgs args, string errorMessage) =>
            args.DeadLetterMessageAsync(args.Message, "Invalid message", errorMessage);
    }
}
