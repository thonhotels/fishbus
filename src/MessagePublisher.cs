using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Thon.Hotels.FishBus
{
    public class MessagePublisher
    {
        private readonly MessageSender _client;

        public MessagePublisher(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString) || !connectionString.ToLower().Contains("entitypath"))
                throw new ArgumentNullException($"ConnectionString must be supplied with EnitityPath");

            _client = new MessageSender(new ServiceBusConnectionStringBuilder(connectionString));
        }

        public async Task SendAsync<T>(T message, string correlationId)
        {
            var msg = MessageBuilder.BuildMessage(message, correlationId);
            await _client.SendAsync(msg);
        }

        public async Task SendAsync<T>(T message) => await SendAsync(message, string.Empty);
    }
}
