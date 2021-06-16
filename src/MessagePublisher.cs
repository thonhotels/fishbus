using Azure.Messaging.ServiceBus;
using System;
using System.Threading.Tasks;
using Azure.Core;

namespace Thon.Hotels.FishBus
{
    public class MessagePublisher
    {
        private readonly ServiceBusClient _client;
        private readonly string _entityPath;

        public MessagePublisher(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString) || !connectionString.ToLower().Contains("entitypath"))
                throw new ArgumentNullException($"ConnectionString must be supplied with EnitityPath");

            _entityPath = ServiceBusConnectionStringProperties.Parse(connectionString).EntityPath;
            _client = new ServiceBusClient(connectionString);
        }
        
        public MessagePublisher(string fullyQualifiedNamespace, string entityPath, TokenCredential tokenCredential)
        {
            if (string.IsNullOrWhiteSpace(fullyQualifiedNamespace))
                throw new ArgumentNullException("fullyQualifiedNamespace must be supplied");
            
            if (string.IsNullOrWhiteSpace(entityPath))
                throw new ArgumentNullException("entityPath must be supplied");
            
            if (tokenCredential == null)
                throw new ArgumentNullException("tokenCredential must not be null");

            _entityPath = entityPath;
            _client = new ServiceBusClient(fullyQualifiedNamespace, tokenCredential);
        }


        public async Task SendScheduledAsync<T>(T message, DateTime time, string correlationId)
        {
            var msg = MessageBuilder.BuildScheduledMessage(message, time, correlationId);
            await Sender.SendMessageAsync(msg);
        }

        public Task SendScheduledAsync<T>(T message, DateTime time) =>
            SendScheduledAsync(message, time, string.Empty);

        public async Task SendWithDelayAsync<T>(T message, TimeSpan timeSpan, string correlationId)
        {
            var msg = MessageBuilder.BuildDelayedMessage(message, timeSpan, correlationId);
            await Sender.SendMessageAsync(msg);
        }

        public async Task SendWithDelayAsync<T>(T message, TimeSpan timeSpan) =>
            await SendWithDelayAsync(message, timeSpan, string.Empty);

        public async Task SendAsync<T>(T message, string correlationId)
        {
            var msg = MessageBuilder.BuildMessage(message, correlationId);
            await Sender.SendMessageAsync(msg);
        }

        public async Task SendAsync<T>(T message) => await SendAsync(message, string.Empty);

        private ServiceBusSender Sender =>
            _client.CreateSender(_entityPath);
    }
}
