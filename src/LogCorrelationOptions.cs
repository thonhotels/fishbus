using System;
using Microsoft.Azure.ServiceBus;
using Serilog.Context;

namespace Thon.Hotels.FishBus
{
    public class LogCorrelationOptions
    {
        internal Func<Message, IDisposable> PushToLogContext { get; set; }

        public LogCorrelationOptions(bool useCorrelationLogging = true)
        {
            if (!useCorrelationLogging)
            {
                PushToLogContext = (message) => new EmptyContextPusher();
            }
            else
            {
                PushToLogContext = CreatePushToLogContext("CorrelationId", "logCorrelationId");
            }
        }

        public LogCorrelationOptions(string logPropertyName, string messagePropertyName)
        {
            PushToLogContext = CreatePushToLogContext(logPropertyName, messagePropertyName);
        }

        private static Func<Message, IDisposable> CreatePushToLogContext(string logPropertyName, string messagePropertyName) =>
            (message) =>
            {
                var logCorrelationId = message.UserProperties.ContainsKey(messagePropertyName)
                    ? message.UserProperties[messagePropertyName]
                    : Guid.NewGuid();
                return LogContext.PushProperty(logPropertyName, logCorrelationId);
            };
    }

    internal class EmptyContextPusher : IDisposable
    {
        public void Dispose()
        {
        }
    }
}