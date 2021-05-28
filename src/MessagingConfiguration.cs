using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Thon.Hotels.FishBus.Options;

namespace Thon.Hotels.FishBus
{
    public class MessagingConfiguration
    {
        public IEnumerable<MessageDispatcher> Dispatchers { get; private set; }

        public MessagingConfiguration(IOptions<MessageSources> messageSources, MessageHandlerRegistry registry, IServiceScopeFactory scopeFactory, LogCorrelationHandler logCorrelationHandler)
        {
            ServiceBusClient CreateClient(MessageSource s)
            {
                if (!string.IsNullOrEmpty(s.ConnectionString))
                    new ServiceBusClient(s.ConnectionString);
                if (s.CredentialType == nameof(AzureCliCredential))
                    new ServiceBusClient(s.Namespace, new AzureCliCredential());
                if (s.CredentialType == nameof(DefaultAzureCredential))
                    new ServiceBusClient(s.Namespace, new DefaultAzureCredential());
                throw new InvalidOperationException("Could not create ServiceBusClient");
            }

            string GetEntityName(MessageSource s) =>
                string.IsNullOrEmpty(s.EntityName) ?
                    ServiceBusConnectionStringProperties.Parse(s.ConnectionString).EntityPath :
                    s.EntityName;
            
            (ServiceBusClient,ServiceBusProcessor) CreateSubscriptionClient(Subscription s)
            {
                var client = CreateClient(s);
                return (client, client.CreateProcessor(GetEntityName(s), s.Name, 
                    new ServiceBusProcessorOptions
                    {
                        AutoCompleteMessages = false,
                        MaxConcurrentCalls = s.MaxConcurrentCalls
                    }));
            }

            (ServiceBusClient,ServiceBusProcessor) CreateQueueClient(Queue q) 
            {
                var client = CreateClient(q);
                return (client, client.CreateProcessor(GetEntityName(q),
                    new ServiceBusProcessorOptions
                    {
                        AutoCompleteMessages = false,
                        MaxConcurrentCalls = q.MaxConcurrentCalls
                    }));
            }

            Dispatchers = messageSources
                .Value
                .Subscriptions
                .Select(subscription => new MessageDispatcher(scopeFactory, CreateSubscriptionClient(subscription), registry, logCorrelationHandler))
                .Concat(
                    messageSources
                        .Value
                        .Queues
                        .Select(queue => new MessageDispatcher(scopeFactory, CreateQueueClient(queue), registry, logCorrelationHandler))
                )
                .ToList();
        }

        public async Task RegisterMessageHandlers(Func<ProcessErrorEventArgs, Task> exceptionReceivedHandler)
        {
            foreach(var d in Dispatchers)
                await d.RegisterMessageHandler(exceptionReceivedHandler);
        }

        public async Task Close()
        {
            await Task.WhenAll(
                Dispatchers
                .Select(async d => await d.Close())
                .ToArray()
            );
        }
    }
}
