using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Thon.Hotels.FishBus
{   
    public static class SercicesExtensions
    { 
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