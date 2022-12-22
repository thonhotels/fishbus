using Azure.Messaging.ServiceBus;
using System;
using System.Threading.Tasks;
using Azure.Core;

namespace Thon.Hotels.FishBus;

public class MessagePublisher
{
    private readonly ServiceBusSender _sender;

    public MessagePublisher(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString) || !connectionString.ToLower().Contains("entitypath"))
            throw new ArgumentNullException($"ConnectionString must be supplied with EnitityPath");

        var entityPath = ServiceBusConnectionStringProperties.Parse(connectionString).EntityPath;
        var client = new ServiceBusClient(connectionString);
        _sender = client.CreateSender(entityPath);
    }

    public MessagePublisher(string fullyQualifiedNamespace, string entityPath, TokenCredential tokenCredential)
    {
        if (string.IsNullOrWhiteSpace(fullyQualifiedNamespace))
            throw new ArgumentNullException("fullyQualifiedNamespace must be supplied");

        if (string.IsNullOrWhiteSpace(entityPath))
            throw new ArgumentNullException("entityPath must be supplied");

        if (tokenCredential == null)
            throw new ArgumentNullException("tokenCredential must not be null");

        var client = new ServiceBusClient(fullyQualifiedNamespace, tokenCredential);
        _sender = client.CreateSender(entityPath);
    }


    public async Task SendScheduledAsync<T>(T message, DateTime time, string correlationId)
    {
        var msg = MessageBuilder.BuildScheduledMessage(message, time, correlationId);
        await _sender.SendMessageAsync(msg);
    }

    public Task SendScheduledAsync<T>(T message, DateTime time) =>
        SendScheduledAsync(message, time, string.Empty);

    public async Task SendWithDelayAsync<T>(T message, TimeSpan timeSpan, string correlationId)
    {
        var msg = MessageBuilder.BuildDelayedMessage(message, timeSpan, correlationId);
        await _sender.SendMessageAsync(msg);
    }

    public async Task SendWithDelayAsync<T>(T message, TimeSpan timeSpan) =>
        await SendWithDelayAsync(message, timeSpan, string.Empty);

    public async Task SendAsync<T>(T message, string correlationId)
    {
        var msg = MessageBuilder.BuildMessage(message, correlationId);
        await _sender.SendMessageAsync(msg);
    }

    public async Task SendAsync<T>(T message) => await SendAsync(message, string.Empty);
}