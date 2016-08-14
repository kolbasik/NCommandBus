using System;
using System.Threading.Tasks;
using kolbasik.NCommandBus.Abstractions;
using Sample.Commands;

namespace Sample.Handles
{
    public sealed class AppDataHandler: ICommandHandler<GetAppName, GetAppName.Result>
    {
        public Task<GetAppName.Result> Handle(CommandContext<GetAppName> context)
        {
            var result = new GetAppName.Result { AppName = AppDomain.CurrentDomain.FriendlyName };
            return Task.FromResult(result);
        }
    }
}