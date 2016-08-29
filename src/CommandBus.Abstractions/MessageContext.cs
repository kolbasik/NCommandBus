using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace kolbasik.NCommandBus.Abstractions
{
    public sealed class MessageContext<TMessage, TResult>
    {
        public MessageContext(TMessage message)
        {
            Message = message;
            ValidationResults = new List<ValidationResult>();
        }

        public List<ValidationResult> ValidationResults { get; }
        public TMessage Message { get; }
        public TResult Result { get; set; }
    }
}