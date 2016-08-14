using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace kolbasik.NCommandBus.Abstractions
{
    public sealed class CommandContext<TCommand, TResult>
    {
        public CommandContext(TCommand command)
        {
            Command = command;
            ValidationResults = new List<ValidationResult>();
        }

        public TCommand Command { get; }
        public TResult Result { get; set; }
        public List<ValidationResult> ValidationResults { get; }
    }
}