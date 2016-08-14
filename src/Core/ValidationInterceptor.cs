using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using kolbasik.NCommandBus.Abstractions;

namespace kolbasik.NCommandBus.Core
{
    public sealed class ValidationInterceptor : ICommandValidator, ICommandObserver
    {
        private static readonly Task Done = Task.FromResult(1);
        public static readonly ValidationInterceptor Instance = new ValidationInterceptor();

        public Task Validate<TCommand>(CommandContext<TCommand> context)
        {
            var validationContext = new ValidationContext(context.Command);
            Validator.TryValidateObject(context.Command, validationContext, context.ValidationResults, true);
            return Done;
        }

        public Task PreInvoke<TResult, TCommand>(CommandContext<TCommand> context)
        {
            if (context.ValidationResults.Count > 0)
                throw new ValidationException(context.Command, context.ValidationResults);
            return Done;
        }

        public Task PostInvoke<TResult, TCommand>(CommandContext<TCommand> context)
        {
            return Done;
        }
    }
}