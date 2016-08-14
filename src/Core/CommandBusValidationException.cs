using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace kolbasik.NCommandBus.Core
{
    public sealed class CommandBusValidationException : ApplicationException
    {
        public CommandBusValidationException(object command, IEnumerable<ValidationResult> validationResults)
        {
            Command = command;
            ValidationResults = new List<ValidationResult>(validationResults);
        }

        public object Command { get; }
        public List<ValidationResult> ValidationResults { get; }
    }
}