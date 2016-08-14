using System.Threading.Tasks;
using kolbasik.NCommandBus.Abstractions;
using Sample.Commands;

namespace Sample.Handles
{
    public sealed class Calculator : ICommandHandler<AddValues, AddValuesResult>
    {
        public Task<AddValuesResult> Handle(CommandContext<AddValues> context)
        {
            var command = context.Command;
            var result = new AddValuesResult {Result = command.A + command.B};
            return Task.FromResult(result);
        }
    }
}
