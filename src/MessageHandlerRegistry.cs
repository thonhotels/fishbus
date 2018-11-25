using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Thon.Hotels.FishBus
{
    public class MessageHandlerRegistry
    {
        private Dictionary<Type, ICollection<Type>> MessageHandlers { get; }

        public MessageHandlerRegistry(Func<IEnumerable<Type>> messageHandlerTypes)
        {
            MessageHandlers = new Dictionary<Type, ICollection<Type>>();
            Init(messageHandlerTypes);
        }

        private void Init(Func<IEnumerable<Type>> messageHandlerTypes)
        {
            messageHandlerTypes()
                .SelectMany(t => GetHandledCommands(t))
                .ToList()
                .ForEach(AddHandledCommand);
        }

        private IEnumerable<(Type handler, Type message)> GetHandledCommands(Type messageHandlerType) =>
            messageHandlerType
                            .GetInterfaces()
                            .Where(i => typeof(IHandleMessage<>).IsAssignableFrom(i.GetGenericTypeDefinition()))
                            .Select(i => (handler: messageHandlerType, message: i.GenericTypeArguments.Single()));

        private void AddHandledCommand((Type handler, Type message) x)
        {
            if (!MessageHandlers.ContainsKey(x.message))
                MessageHandlers.Add(x.message, new List<Type>());
            MessageHandlers[x.message].Add(x.handler);
        }

        public IEnumerable<object> GetHandlers(Type messageType, IServiceScope scope) =>
            MessageHandlers.ContainsKey(messageType) ?
                MessageHandlers[messageType]
                    .Select(t => scope.ServiceProvider.GetRequiredService(t)) :
                    new List<object>();

        public Type GetMessageTypeByName(string label) =>
            MessageHandlers
                .Keys
                .FirstOrDefault(type => type.FullName == label);        
    }
}
