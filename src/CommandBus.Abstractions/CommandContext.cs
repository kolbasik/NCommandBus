using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace kolbasik.NCommandBus.Abstractions
{
    public sealed class CommandContext<TCommand, TResult>
    {
        private static readonly Task Done = Task.FromResult(1);

        public CommandContext(TCommand command)
        {
            Command = command;
            ValidationResults = new List<ValidationResult>();
        }

        public TCommand Command { get; }
        public TResult Result { get; set; }
        public List<ValidationResult> ValidationResults { get; }
        public Task CompleteTask => Done;
    }
}