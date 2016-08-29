﻿using System;
using System.Reflection;
using System.Threading;
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
using Sample.Handles;

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
            dependencyResolver.Register(new Shop());
            dependencyResolver.RegisterTypes(typeof(IQueryHandler<,>), Assembly.Load("Sample.Handles"));
            dependencyResolver.RegisterTypes(typeof(ICommandHandler<>), Assembly.Load("Sample.Handles"));

            try
            {
                Console.WriteLine(@"SelfHost:");
                var hostCommandBus = new MessageBus(new InProcessMessageInvoker(dependencyResolver.ServiceContainer));
                await Perform(hostCommandBus).ConfigureAwait(false);

                Console.WriteLine(@"Http:");
                var httpCommandBus = new MessageBus(new HttpMessageInvoker(new Uri(@"http://localhost:58452/MessageBus.ashx")));
                await Perform(httpCommandBus).ConfigureAwait(false);

                //Console.WriteLine(@"Remote:");
                //using (var channel = RemoteChannel.Tcp())
                //{
                //    var remoteCommandBus = new MessageBus(new RemoteMessageInvoker(channel, "tcp://localhost:8081/RPC"));
                //    await Perform(remoteCommandBus).ConfigureAwait(false);
                //}

                //Console.WriteLine(@"MassTransit:");
                //var busControl = Bus.Factory.CreateUsingRabbitMq(x =>
                //{
                //    x.Host(new Uri("rabbitmq://localhost:15672/"), h =>
                //    {
                //        h.Username("guest");
                //        h.Password("guest");
                //    });
                //});
                //await busControl.StartAsync().ConfigureAwait(false);

                //var massTransitCommandBus = new MessageBus(new MassTransitMessageInvoker(busControl, new Uri(@"rabbitmq://localhost:15672/CommandBus"), TimeSpan.FromSeconds(15)));
                //await Perform(massTransitCommandBus).ConfigureAwait(false);

                //await busControl.StopAsync().ConfigureAwait(false);
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

        private async Task Perform(IMessageBus messageBus)
        {
            var appNameResult = await messageBus.Ask<GetAppName.Result, GetAppName>(new GetAppName()).ConfigureAwait(false);
            Console.WriteLine($"AppName: {appNameResult.AppName}");

            var addValues = new GetAddedValues {A = 2, B = 3};
            var addValuesResult = await messageBus.Ask<GetAddedValuesResult, GetAddedValues>(addValues).ConfigureAwait(false);
            Console.WriteLine($"{addValues.A} + {addValues.B} = {addValuesResult.Result}");

            var subValues = new GetSubtractedValues { A = 10, B = 7 };
            var subValuesResult = await messageBus.Ask<GetSubtractedValuesResult, GetSubtractedValues>(subValues).ConfigureAwait(false);
            Console.WriteLine($"{subValues.A} - {subValues.B} = {subValuesResult.Result}");

            const string customerEmail = "james_bond@mi6.com";
            var createOrder = new ShopApi.CreateOrder { CustomerEmail = customerEmail, ProductName = "mug", Quantity = 1 };
            await messageBus.Tell(createOrder, CancellationToken.None).ConfigureAwait(false);
            Console.WriteLine($"Customer ( {customerEmail} ) has ordered {createOrder.Quantity} {createOrder.ProductName}(s).");

            var customerOrders = await messageBus.Ask<ShopApi.GetCustomerOrdersResult, ShopApi.GetCustomerOrders>(new ShopApi.GetCustomerOrders(customerEmail)).ConfigureAwait(false);
            foreach (var customerOrder in customerOrders.Orders)
            {
                Console.WriteLine($"{customerOrders.CustomerEmail}: {customerOrder.ProductName} {customerOrder.Quantity}");
            }
        }
    }
}