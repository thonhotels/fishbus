using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Thon.Hotels.FishBus.Options;

namespace Thon.Hotels.FishBus;

public static class ServicesExtensions
{
    /// Scan the given assembly or the calling assembly for MessageHandlers
    /// and register them in the .NET Core IoC.
    /// Fishbus framework classes are also registered
    /// <param name="assembly">Assembly that contains the MessageHandlers</param>
    public static IServiceCollection ConfigureMessaging(this IServiceCollection services)
    {
        return services.ConfigureMessaging(new MessagingOptions
        {
            WithCorrelationLogging = false,
            Assemblies = new[] {Assembly.GetCallingAssembly()}
        });
    }

    public static IServiceCollection ConfigureMessaging(this IServiceCollection services, params Assembly[] assemblies)
    {
        return services.ConfigureMessaging(new MessagingOptions
        {
            WithCorrelationLogging = false,
            Assemblies = assemblies
        });
    }

    public static IServiceCollection ConfigureMessagingWithCorrelationLogging(this IServiceCollection services)
    {
        return services.ConfigureMessaging(new MessagingOptions
        {
            WithCorrelationLogging = true,
            Assemblies = new[] {Assembly.GetCallingAssembly()}
        });
    }

    public static IServiceCollection ConfigureMessagingWithCorrelationLogging(this IServiceCollection services, params Assembly[] assemblies)
    {
        return services.ConfigureMessaging(new MessagingOptions
        {
            WithCorrelationLogging = true,
            Assemblies = assemblies
        });
    }

    public static IServiceCollection ConfigureMessagingWithCorrelationLogging(this IServiceCollection services, LogCorrelationOptions logCorrelationOptions)
    {
        return services.ConfigureMessaging(new MessagingOptions
        {
            WithCorrelationLogging = true,
            LogCorrelationOptions = logCorrelationOptions,
            Assemblies = new[] {Assembly.GetCallingAssembly()}
        });
    }

    public static IServiceCollection ConfigureMessagingWithCorrelationLogging(this IServiceCollection services, LogCorrelationOptions logCorrelationOptions, params Assembly[] assemblies)
    {
        return services.ConfigureMessaging(new MessagingOptions
        {
            WithCorrelationLogging = true,
            LogCorrelationOptions = logCorrelationOptions
        });
    }

    public static IServiceCollection ConfigureMessaging(this IServiceCollection services, MessagingOptions messagingOptions)
    {
        var logCorrelationHandler = new LogCorrelationHandler(messagingOptions.WithCorrelationLogging, messagingOptions.LogCorrelationOptions);
        services.AddOptions<MessagingOptions>()
            .Configure(options =>
            {
                options.Assemblies = messagingOptions.Assemblies;
                options.TokenCredential = messagingOptions.TokenCredential;
                options.FullyQualifiedNamespace = messagingOptions.FullyQualifiedNamespace;
                options.LogCorrelationOptions = messagingOptions.LogCorrelationOptions;
                options.WithCorrelationLogging = messagingOptions.WithCorrelationLogging;
            });
        if (logCorrelationHandler == null)
            throw new ArgumentNullException(nameof(logCorrelationHandler));
        MessageHandlerTypes
            .GetAll(messagingOptions.Assemblies)
            .ToList()
            .ForEach(t => services.AddTransient(t));
        services
            .AddSingleton<MessagingConfiguration>()
            .AddSingleton(p => new MessageHandlerRegistry(() => MessageHandlerTypes.GetAll(messagingOptions.Assemblies)))
            .AddSingleton(logCorrelationHandler)
            .AddHostedService<MessagingService>();

        return services;
    }
}