using System.Threading.Tasks;

namespace kolbasik.NCommandBus.Abstractions
{
    public interface IMessageObserver
    {
        Task PreInvoke<TResult, TMessage>(MessageContext<TMessage, TResult> context);
        Task PostInvoke<TResult, TMessage>(MessageContext<TMessage, TResult> context);
    }
}