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
            var logCorrelationHandler = new LogCorrelationHandler(false);
            return services.ConfigureMessaging(logCorrelationHandler, assembly);
        }

        public static IServiceCollection ConfigureMessagingWithCorrelationLogging(this IServiceCollection services, Assembly assembly = null)
        {
            var logCorrelationHandler = new LogCorrelationHandler(true);
            return services.ConfigureMessaging(logCorrelationHandler, assembly);
        }

        public static IServiceCollection ConfigureMessagingWithCorrelationLogging(this IServiceCollection services, LogCorrelationOptions logCorrelationOptions, Assembly assembly = null)
        {
            var logCorrelationHandler = new LogCorrelationHandler(true, logCorrelationOptions);
            return services.ConfigureMessaging(logCorrelationHandler, assembly);
        }

        private static IServiceCollection ConfigureMessaging(this IServiceCollection services, LogCorrelationHandler logCorrelationHandler, Assembly assembly = null)
        {
            if (logCorrelationHandler == null)
                throw new ArgumentNullException(nameof(logCorrelationHandler));
            assembly = assembly ?? Assembly.GetCallingAssembly();
            MessageHandlerTypes
                .GetAll(assembly)
                .ToList()
                .ForEach(t => services.AddTransient(t));
            services
                .AddSingleton<MessagingConfiguration>()
                .AddSingleton(p => new MessageHandlerRegistry(() => MessageHandlerTypes.GetAll(assembly)))
                .AddSingleton(logCorrelationHandler)
                .AddHostedService<MessagingService>();

            return services;
        }
    }
}