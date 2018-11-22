using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Thon.Hotels.FishBus.Options;

namespace Thon.Hotels.FishBus
{
    public class MessagingConfiguration
    {
        public IEnumerable<MessageDispatcher> Dispatchers { get; private set; }

        public MessagingConfiguration(IOptions<MessageSources> messageSources, MessageHandlerRegistry registry)
        {
            SubscriptionClient CreateSubscriptionClient(Subscription s)
            {
                var (connectionString, entityPath) = ConnectionStringSplitter.Split(s.ConnectionString);
                return new SubscriptionClient(connectionString, entityPath, s.Name);
            }

            QueueClient CreateQueueClient(Queue q)
            {
                var (connectionString, entityPath) = ConnectionStringSplitter.Split(q.ConnectionString);
                return new QueueClient(connectionString, entityPath);
            }

            IServiceScopeFactory scopeFactory = null;
            Dispatchers = messageSources
                .Value
                .Subscriptions
                .Select(subscription => new MessageDispatcher(scopeFactory, CreateSubscriptionClient(subscription), registry))
                .Concat(
                    messageSources
                        .Value
                        .Queues
                        .Select(queue => new MessageDispatcher(scopeFactory, CreateQueueClient(queue), registry))
                )
                .ToList();
        }

        public void RegisterMessageHandlers(Func<ExceptionReceivedEventArgs, Task> exceptionReceivedHandler)
        {
            Dispatchers
                .ToList()
                .ForEach(d => d.RegisterMessageHandler(exceptionReceivedHandler));
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
