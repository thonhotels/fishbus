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
        public static IServiceCollection ConfigureMessaging(this IServiceCollection services)
        {
            var logCorrelationHandler = new LogCorrelationHandler(false);
            return services.ConfigureMessaging(logCorrelationHandler, new[] {Assembly.GetCallingAssembly()});
        }

        public static IServiceCollection ConfigureMessaging(this IServiceCollection services, params Assembly[] assemblies)
        {
            var logCorrelationHandler = new LogCorrelationHandler(false);
            return services.ConfigureMessaging(logCorrelationHandler, assemblies);
        }

        public static IServiceCollection ConfigureMessagingWithCorrelationLogging(this IServiceCollection services)
        {
            var logCorrelationHandler = new LogCorrelationHandler(true);
            return services.ConfigureMessaging(logCorrelationHandler, new[] {Assembly.GetCallingAssembly()});
        }

        public static IServiceCollection ConfigureMessagingWithCorrelationLogging(this IServiceCollection services, params Assembly[] assemblies)
        {
            var logCorrelationHandler = new LogCorrelationHandler(true);
            return services.ConfigureMessaging(logCorrelationHandler, assemblies);
        }

        public static IServiceCollection ConfigureMessagingWithCorrelationLogging(this IServiceCollection services, LogCorrelationOptions logCorrelationOptions)
        {
            var logCorrelationHandler = new LogCorrelationHandler(true, logCorrelationOptions);
            return services.ConfigureMessaging(logCorrelationHandler, new[] {Assembly.GetCallingAssembly()});
        }

        public static IServiceCollection ConfigureMessagingWithCorrelationLogging(this IServiceCollection services, LogCorrelationOptions logCorrelationOptions, params Assembly[] assemblies)
        {
            var logCorrelationHandler = new LogCorrelationHandler(true, logCorrelationOptions);
            return services.ConfigureMessaging(logCorrelationHandler, assemblies);
        }

        private static IServiceCollection ConfigureMessaging(this IServiceCollection services, LogCorrelationHandler logCorrelationHandler, Assembly[] assemblies)
        {
            if (logCorrelationHandler == null)
                throw new ArgumentNullException(nameof(logCorrelationHandler));
            MessageHandlerTypes
                .GetAll(assemblies)
                .ToList()
                .ForEach(t => services.AddTransient(t));
            services
                .AddSingleton<MessagingConfiguration>()
                .AddSingleton(p => new MessageHandlerRegistry(() => MessageHandlerTypes.GetAll(assemblies)))
                .AddSingleton(logCorrelationHandler)
                .AddHostedService<MessagingService>();

            return services;
        }
    }
}