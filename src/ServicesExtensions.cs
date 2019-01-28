using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Thon.Hotels.FishBus
{   
    public static class ServicesExtensions
    {
        /// Scan the given assembly or the calling assembly for MessageHandlers
        /// and register them in the .NET Core IoC.
        /// Fishbus framework classes are also registered
        /// <param name="assembly">Assembly that contains the MessageHandlers</param>
        public static IServiceCollection ConfigureMessaging(this IServiceCollection services, Assembly assembly = null)
        {
            var logCorrelationOptions = new LogCorrelationOptions(false);
            return services.ConfigureMessaging(logCorrelationOptions, assembly);
        }

        public static IServiceCollection ConfigureMessagingWithCorrelationLogging(this IServiceCollection services, Assembly assembly = null)
        {
            var logCorrelationOptions = new LogCorrelationOptions();
            return services.ConfigureMessaging(logCorrelationOptions, assembly);
        }

        public static IServiceCollection ConfigureMessagingWithCorrelationLogging(this IServiceCollection services, string logPropertyName, string messagePropertyName, Assembly assembly = null)
        {
            var logCorrelationOptions = new LogCorrelationOptions(logPropertyName, messagePropertyName);
            return services.ConfigureMessaging(logCorrelationOptions, assembly);
        }

        private static IServiceCollection ConfigureMessaging(this IServiceCollection services, LogCorrelationOptions logCorrelationOptions, Assembly assembly = null)
        {
            if (logCorrelationOptions == null)
                throw new ArgumentNullException(nameof(logCorrelationOptions));
            assembly = assembly ?? Assembly.GetCallingAssembly();
            MessageHandlerTypes
                .GetAll(assembly)
                .ToList()
                .ForEach(t => services.AddTransient(t));
            services
                .AddSingleton<MessagingConfiguration>()
                .AddSingleton(p => new MessageHandlerRegistry(() => MessageHandlerTypes.GetAll(assembly)))
                .AddSingleton(logCorrelationOptions)
                .AddHostedService<MessagingService>();

            return services;
        }
    }
}