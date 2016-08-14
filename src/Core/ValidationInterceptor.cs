using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using kolbasik.NCommandBus.Abstractions;

namespace kolbasik.NCommandBus.Core
{
    public sealed class ValidationInterceptor : ICommandValidator, ICommandObserver
    {
        public static readonly ValidationInterceptor Instance = new ValidationInterceptor();
        private static readonly Task Done = Task.FromResult(1);

        public Task Validate<TCommand, TResult>(CommandContext<TCommand, TResult> context)
        {
            var validationContext = new ValidationContext(context.Command);
            Validator.TryValidateObject(context.Command, validationContext, context.ValidationResults, true);
            return Done;
        }

        public Task PreInvoke<TCommand, TResult>(CommandContext<TCommand, TResult> context)
        {
            if (context.ValidationResults.Count > 0)
                throw new ValidationException(context.Command, context.ValidationResults);
            return context.CompleteTask;
        }

        public Task PostInvoke<TCommand, TResult>(CommandContext<TCommand, TResult> context)
        {
            return context.CompleteTask;
        }
    }
}