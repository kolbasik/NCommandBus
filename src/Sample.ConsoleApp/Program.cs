using System;
using System.Reflection;
using System.Threading.Tasks;
using kolbasik.NCommandBus.Abstractions;
using kolbasik.NCommandBus.Core;
using kolbasik.NCommandBus.Host;
using kolbasik.NCommandBus.Http;
using kolbasik.NCommandBus.MassTransit;
using kolbasik.NCommandBus.Remote;
using MassTransit;
using Sample.Commands;
using Sample.Core;

namespace Sample.ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            new Program().Run().GetAwaiter().GetResult();
        }

        private async Task Run()
        {
            Console.WriteLine(@"Press any key to start...");
            Console.ReadKey(true);

            var dependencyResolver = new SampleDependencyResolver();
            dependencyResolver.RegisterAll(typeof(ICommandHandler<,>), Assembly.Load("Sample.Handles"));

            try
            {
                Console.WriteLine(@"SelfHost:");
                var hostCommandBus = new CommandBus(new InProcessCommandInvoker(dependencyResolver.ServiceContainer));
                await Perform(hostCommandBus).ConfigureAwait(false);

                Console.WriteLine(@"Http:");
                var httpCommandBus = new CommandBus(new HttpCommandInvoker(new Uri(@"http://localhost:58452/CommandBus.ashx")));
                await Perform(httpCommandBus).ConfigureAwait(false);

                Console.WriteLine(@"Remote:");
                using (var channel = RemoteChannel.Tcp())
                {
                    var remoteCommandBus = new CommandBus(new RemoteCommandInvoker(channel, "tcp://localhost:8081/RPC"));
                    await Perform(remoteCommandBus).ConfigureAwait(false);
                }

                Console.WriteLine(@"MassTransit:");
                var busControl = Bus.Factory.CreateUsingRabbitMq(x =>
                {
                    x.Host(new Uri("rabbitmq://localhost:15672/"), h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });
                });
                await busControl.StartAsync().ConfigureAwait(false);

                var massTransitCommandBus = new CommandBus(new MassTransitCommandInvoker(busControl, new Uri(@"rabbitmq://localhost:15672/CommandBus"), TimeSpan.FromSeconds(15)));
                await Perform(massTransitCommandBus).ConfigureAwait(false);

                await busControl.StopAsync().ConfigureAwait(false);
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

        private async Task Perform(CommandBus commandBus)
        {
            var appNameResult = await commandBus.Send<GetAppName.Result, GetAppName>(new GetAppName()).ConfigureAwait(false);
            Console.WriteLine($"AppName: {appNameResult.AppName}");

            var addValues = new AddValues {A = 2, B = 3};
            var addValuesResult = await commandBus.Send<AddValuesResult, AddValues>(addValues).ConfigureAwait(false);
            Console.WriteLine($"{addValues.A} + {addValues.B} = {addValuesResult.Result}");

            var subValues = new SubValues { A = 10, B = 7 };
            var subValuesResult = await commandBus.Send<SubValuesResult, SubValues>(subValues).ConfigureAwait(false);
            Console.WriteLine($"{subValues.A} - {subValues.B} = {subValuesResult.Result}");
        }
    }
}