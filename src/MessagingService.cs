using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Thon.Hotels.FishBus
{
    /// The main/singelton service that contains all messaging clients
    public class MessagingService : IHostedService
    {
        private MessagingConfiguration Configuration { get; }

        public MessagingService(MessagingConfiguration configuration)
        {
            Configuration = configuration;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                Configuration.RegisterMessageHandlers(ExceptionReceivedHandler);
                return Task.CompletedTask;
            }
            catch (Exception exception)
            {
                Log.Error($"Error registering message handler", exception);
                return Task.CompletedTask;
            }
        }

        Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Log.Error(exceptionReceivedEventArgs.Exception,
                        $@"Message handler encountered an exception.
                        Endpoint: {context.Endpoint}
                        Entity Path: {context.EntityPath}
                        Executing Action: {context.Action}");

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Information("Signal received. Gracefully shutting down.");
            await Configuration.Close();
            Thread.Sleep(1000);
        }
    }
}
