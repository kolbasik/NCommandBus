using System;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using kolbasik.NCommandBus.Abstractions;
using kolbasik.NCommandBus.Core;
using kolbasik.NCommandBus.Host;
using kolbasik.NCommandBus.Http;
using Sample.Commands;
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

            var serviceContainer = new ServiceContainer();
            serviceContainer.AddService(typeof (ICommandHandler<AddValues, AddValuesResult>), new Calculator());
            serviceContainer.AddService(typeof (ICommandHandler<SubValues, SubValuesResult>), new Calculator());
            serviceContainer.AddService(typeof (ICommandHandler<GetAppName, GetAppName.Result>), new AppDataHandler());
            try
            {
                var hostCommandBus = new CommandBus(new HostCommandInvoker(serviceContainer));
                await Perform(hostCommandBus).ConfigureAwait(false);

                var httpCommandBus = new CommandBus(new HttpCommandInvoker(new Uri(@"http://localhost:58452/CommandBus.ashx")));
                await Perform(httpCommandBus).ConfigureAwait(false);
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

            var subValues = new AddValues {A = 10, B = 7};
            var subValuesResult = await commandBus.Send<AddValuesResult, AddValues>(subValues).ConfigureAwait(false);
            Console.WriteLine($"{subValues.A} + {subValues.B} = {subValuesResult.Result}");
        }
    }
}