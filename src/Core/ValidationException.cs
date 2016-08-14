using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace kolbasik.NCommandBus.Core
{
    public sealed class ValidationException : ApplicationException
    {
        public ValidationException(object command, IEnumerable<ValidationResult> validationResults)
        {
            Command = command;
            ValidationResults = new List<ValidationResult>(validationResults);
        }

        public object Command { get; }
        public List<ValidationResult> ValidationResults { get; }
    }
}