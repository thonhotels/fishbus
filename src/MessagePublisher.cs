using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;
using System;
using System.Reflection;
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

            var label = message.GetType().GetCustomAttribute<MessageLabelAttribute>()?.Label;

            if (string.IsNullOrWhiteSpace(label))
                throw new Exception($"Label must be specified on {message.GetType().Name} using MessageLabelAttribute");


            await _client.SendAsync(new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)))
            {
                Label = label
            });
        }
    }
}
