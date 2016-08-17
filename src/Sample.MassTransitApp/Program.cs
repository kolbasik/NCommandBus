using System;
using System.Reflection;
using System.Threading.Tasks;
using kolbasik.NCommandBus.Abstractions;
using kolbasik.NCommandBus.Core;
using kolbasik.NCommandBus.Host;
using kolbasik.NCommandBus.MassTransit;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Sample.Core;

namespace Sample.MassTransitApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            new Program().Run().GetAwaiter().GetResult();
        }

        private async Task Run()
        {
            var dependencyResolver = new SampleDependencyResolver();
            dependencyResolver.RegisterAll(typeof(ICommandHandler<,>), Assembly.Load("Sample.Handles"));

            var hostCommandBus = new CommandBus(new HostCommandInvoker(dependencyResolver.ServiceContainer));
            try
            {
                var busControl = Bus.Factory.CreateUsingRabbitMq(x =>
                {
                    var host = x.Host(
                        new Uri("rabbitmq://localhost:15672/"),
                        h =>
                        {
                            h.Username("guest");
                            h.Password("guest");
                        });

                    x.ReceiveEndpoint(host, "CommandBus",
                        e =>
                        {
                            foreach (var consumer in CommandBusConsumerFactory.CreateConsumers(hostCommandBus, dependencyResolver.Definitions))
                            {
                                e.Instance(consumer);
                            }
                        });
                });

                busControl.Start();

                Console.WriteLine(@"Press any key to disconnect...");
                Console.ReadKey(true);

                busControl.Stop();
            }
            catch (Exception ex)
            {
                do
                {
                    Console.WriteLine(ex.ToString());
                } while ((ex = ex.InnerException) != null);
            }

            Console.WriteLine(@"Press any key to exit...");
            Console.ReadKey(true);
        }
    }
}
