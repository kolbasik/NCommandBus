using System;
using System.Collections.Generic;
using System.Linq;
using kolbasik.NCommandBus.Abstractions;
using MassTransit;

namespace kolbasik.NCommandBus.MassTransit
{
    public static class CommandBusConsumerFactory
    {
        public static IEnumerable<IConsumer> CreateConsumers(IQueryBus commandBus, IEnumerable<Type> definitions)
        {
            var commandHandlerTypes = definitions.Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>));
            foreach (var commandHandlerType in commandHandlerTypes)
            {
                var commandType = commandHandlerType.GenericTypeArguments[0];
                var resultType = commandHandlerType.GenericTypeArguments[1];
                yield return CreateConsumer(commandBus, commandType, resultType);
            }
        }

        public static IConsumer CreateConsumer(IQueryBus commandBus, Type commandType, Type resultType)
        {
            var consumerType = typeof(CommandBusConsumer<,>).MakeGenericType(commandType, resultType);
            var consumer = Activator.CreateInstance(consumerType, commandBus);
            return (IConsumer)consumer;
        }
    }
}