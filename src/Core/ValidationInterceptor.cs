using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using kolbasik.NCommandBus.Abstractions;

namespace kolbasik.NCommandBus.Core
{
    public sealed class ValidationInterceptor : IMessageValidator, IMessageObserver
    {
        private static readonly Task Done = Task.FromResult(1);
        public static readonly ValidationInterceptor Instance = new ValidationInterceptor();

        public Task Validate<TMessage, TResult>(MessageContext<TMessage, TResult> context)
        {
            var validationContext = new ValidationContext(context.Message);
            Validator.TryValidateObject(context.Message, validationContext, context.ValidationResults, true);
            return Done;
        }

        public Task PreInvoke<TResult, TMessage>(MessageContext<TMessage, TResult> context)
        {
            if (context.ValidationResults.Count > 0)
                throw new ValidationException(context.Message, context.ValidationResults);
            return Done;
        }

        public Task PostInvoke<TResult, TMessage>(MessageContext<TMessage, TResult> context)
        {
            return Done;
        }
    }
}