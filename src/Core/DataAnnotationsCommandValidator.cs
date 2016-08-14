using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using kolbasik.NCommandBus.Abstractions;

namespace kolbasik.NCommandBus.Core
{
    public sealed class DataAnnotationsCommandValidator : ICommandValidator
    {
        public static readonly DataAnnotationsCommandValidator Instance = new DataAnnotationsCommandValidator();
        private static readonly Task Done = Task.FromResult(1);

        public Task Validate<TCommand, TResult>(CommandContext<TCommand, TResult> context)
        {
            var validationContext = new ValidationContext(context.Command);
            Validator.TryValidateObject(context.Command, validationContext, context.ValidationResults, true);
            return Done;
        }
    }
}