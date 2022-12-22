using System;
using Azure.Messaging.ServiceBus;
using Serilog.Context;

namespace Thon.Hotels.FishBus;

public class LogCorrelationHandler
{
    internal Func<ServiceBusReceivedMessage, IDisposable> PushToLogContext { get; set; }

    internal LogCorrelationHandler(bool useCorrelationLogging, LogCorrelationOptions options = null)
    {
        if (!useCorrelationLogging)
        {
            PushToLogContext = _ => new EmptyContextPusher();
        }
        else
        {
            var logPropertyName = options?.LogPropertyName ?? "CorrelationId";
            var messagePropertyName = options?.MessagePropertyName ?? "logCorrelationId";

            PushToLogContext =
                CreatePushToLogContext(logPropertyName, messagePropertyName, options?.SetCorrelationLogId);
        }
    }

    private static Func<ServiceBusReceivedMessage, IDisposable> CreatePushToLogContext(string logPropertyName,
        string messagePropertyName, Action<string> setCorrelationLogId) =>
        message =>
        {
            var logCorrelationId = message.ApplicationProperties.ContainsKey(messagePropertyName)
                ? message.ApplicationProperties[messagePropertyName]
                : Guid.NewGuid();

            setCorrelationLogId?.Invoke(logCorrelationId.ToString());
            return LogContext.PushProperty(logPropertyName, logCorrelationId);
        };
}

internal class EmptyContextPusher : IDisposable
{
    public void Dispose()
    {
    }
}