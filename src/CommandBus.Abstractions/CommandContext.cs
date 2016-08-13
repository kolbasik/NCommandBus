using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace kolbasik.NCommandBus.Core
{
    public sealed class CommandContext
    {
        public CommandContext(object command)
        {
            Command = command;
            ValidationResults = new List<ValidationResult>();
        }

        public object Command { get; }
        public object Result { get; set; }
        public List<ValidationResult> ValidationResults { get; }
    }
}