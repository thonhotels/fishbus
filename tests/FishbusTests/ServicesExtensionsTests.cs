using System.Reflection;
using FishbusTests.MessageHandlers;
using Microsoft.Extensions.DependencyInjection;
using Thon.Hotels.FishBus;
using Thon.Hotels.FishBus.Options;
using Xunit;

namespace FishbusTests
{
    
    public class ServicesExtensionsTests
    {
        [Fact]
        public void ConfigureMessagingWithCorrelationLogging_LoadsServiceCollection()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.ConfigureMessagingWithCorrelationLogging(new LogCorrelationOptions
            {
                SetCorrelationLogId = s => { }
            });
            serviceCollection.Configure<MessageSources>((m) => { });

            var serviceProvider = serviceCollection.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetRequiredService<MessagingConfiguration>());
            Assert.NotNull(serviceProvider.GetRequiredService<MessageHandlerRegistry>());
            Assert.NotNull(serviceProvider.GetRequiredService<LogCorrelationHandler>());
        }

        [Fact]
        public void ConfigureMessaging_LoadsServiceCollection()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.ConfigureMessaging();
            serviceCollection.Configure<MessageSources>((m) => { });

            var serviceProvider = serviceCollection.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetRequiredService<MessagingConfiguration>());
            Assert.NotNull(serviceProvider.GetRequiredService<MessageHandlerRegistry>());
            Assert.NotNull(serviceProvider.GetRequiredService<LogCorrelationHandler>());
        }

        [Fact]
        public void ConfigureMessaging_GivenAssembly_LoadsServiceCollection()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.ConfigureMessaging(Assembly.GetExecutingAssembly());
            serviceCollection.Configure<MessageSources>((m) => { });

            var serviceProvider = serviceCollection.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetRequiredService<MessagingConfiguration>());
            Assert.NotNull(serviceProvider.GetRequiredService<MessageHandlerRegistry>());
            Assert.NotNull(serviceProvider.GetRequiredService<LogCorrelationHandler>());
        }

        [Fact]
        public void ConfigureMessaging_LoadsMessageHandlers()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.ConfigureMessaging(Assembly.GetExecutingAssembly());
            serviceCollection.Configure<MessageSources>((m) => { });

            var serviceProvider = serviceCollection.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetRequiredService<HandlerA>());
            Assert.NotNull(serviceProvider.GetRequiredService<HandlerB>());
            Assert.NotNull(serviceProvider.GetRequiredService<DuoHandler>());
        }
    }
}