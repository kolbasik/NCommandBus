﻿using System.Threading;
using System.Threading.Tasks;
using kolbasik.NCommandBus.Abstractions;

namespace kolbasik.NCommandBus.Core
{
    public static class CommandBusExtensions
    {
        public static Task<TResult> Send<TResult, TCommand>(this ICommandBus commandBus, TCommand command)
            where TCommand : class
            where TResult : class
        {
            return commandBus.Send<TResult, TCommand>(command, CancellationToken.None);
        }
    }
}