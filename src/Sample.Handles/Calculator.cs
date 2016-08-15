using System.Threading;
using System.Threading.Tasks;
using kolbasik.NCommandBus.Abstractions;
using Sample.Commands;

namespace Sample.Handles
{
    public sealed class Calculator : ICommandHandler<AddValues, AddValuesResult>, ICommandHandler<SubValues, SubValuesResult>
    {
        public Task<AddValuesResult> Handle(AddValues command, CancellationToken cancellationToken)
        {
            var result = new AddValuesResult {Result = command.A + command.B};
            return Task.FromResult(result);
        }

        public Task<SubValuesResult> Handle(SubValues command, CancellationToken cancellationToken)
        {
            var result = new SubValuesResult { Result = command.A - command.B };
            return Task.FromResult(result);
        }
    }
}
