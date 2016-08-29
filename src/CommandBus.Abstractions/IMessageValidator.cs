using System.Threading.Tasks;

namespace kolbasik.NCommandBus.Abstractions
{
    public interface IMessageValidator
    {
        Task Validate<TMessage, TResult>(MessageContext<TMessage, TResult> context);
    }
}