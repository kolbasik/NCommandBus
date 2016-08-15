using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Web;
using kolbasik.NCommandBus.Abstractions;
using kolbasik.NCommandBus.Core;
using kolbasik.NCommandBus.Host;
using Sample.Commands;
using Sample.Handles;

namespace Sample.WebApp
{
    public class Global : HttpApplication
    {
        public static readonly CommandBus CommandBus;
        public static readonly List<Type> CommandHandlerTypes;

        static Global()
        {
            var serviceContainer = new ServiceContainer();
            serviceContainer.AddService(typeof(ICommandHandler<AddValues, AddValuesResult>), new Calculator());
            serviceContainer.AddService(typeof(ICommandHandler<GetAppName, GetAppName.Result>), new AppDataHandler());

            CommandHandlerTypes = new List<Type>
            {
                typeof(ICommandHandler<AddValues, AddValuesResult>),
                typeof(ICommandHandler<GetAppName, GetAppName.Result>)
            };
            CommandBus = new CommandBus(new HostCommandInvoker(serviceContainer));
        }
    }
}