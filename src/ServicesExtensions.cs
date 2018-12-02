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
            assembly = assembly ?? Assembly.GetCallingAssembly();
            MessageHandlerTypes
                .GetAll(assembly)
                .ToList()
                .ForEach(t => services.AddTransient(t));
            services
                .AddSingleton<MessagingConfiguration>()
                .AddSingleton<MessageHandlerRegistry>(p => new MessageHandlerRegistry(() => MessageHandlerTypes.GetAll(assembly)))
                .AddHostedService<MessagingService>();
            return services;
        }
    }
}