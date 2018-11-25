using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Thon.Hotels.FishBus
{   
    public static class SercicesExtensions
    { 
        public static IServiceCollection ConfigureMessaging(this IServiceCollection services)
        {
            MessageHandlerTypes
                .GetAll()
                .ToList()
                .ForEach(t => services.AddTransient(t));
            services
                .AddSingleton<MessagingConfiguration>()
                .AddSingleton<MessageHandlerRegistry>(p => new MessageHandlerRegistry(MessageHandlerTypes.GetAll))
                .AddHostedService<MessagingService>();
            return services;
        }
    }
}