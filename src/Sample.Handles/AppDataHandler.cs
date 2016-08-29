using System;
using System.Threading;
using System.Threading.Tasks;
using kolbasik.NCommandBus.Abstractions;
using Sample.Commands;

namespace Sample.Handles
{
    public sealed class AppDataHandler: IQueryHandler<GetAppName, GetAppName.Result>
    {
        public Task<GetAppName.Result> Handle(GetAppName query, CancellationToken cancellationToken)
        {
            var result = new GetAppName.Result { AppName = AppDomain.CurrentDomain.FriendlyName };
            return Task.FromResult(result);
        }
    }
}