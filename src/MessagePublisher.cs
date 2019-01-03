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


            var (cs, entityPath) = ConnectionStringSplitter.Split(connectionString);
            _client = new MessageSender(cs, entityPath);
        }

        public async Task SendAsync<T>(T message)
        {
            var id = MessageAttributes.GetMessageId(message);
            var label = MessageAttributes.GetMessageLabel(message);

            var msg = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)))
            {
                Label = label
            };

            if (!string.IsNullOrWhiteSpace(id))
                msg.MessageId = id;

            await _client.SendAsync(msg);
        }
    }
}
