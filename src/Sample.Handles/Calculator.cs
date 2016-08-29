using System.Threading;
using System.Threading.Tasks;
using kolbasik.NCommandBus.Abstractions;
using Sample.Commands;

namespace Sample.Handles
{
    public sealed class Calculator : IQueryHandler<AddValues, AddValuesResult>, IQueryHandler<SubValues, SubValuesResult>
    {
        public Task<AddValuesResult> Handle(AddValues query, CancellationToken cancellationToken)
        {
            var result = new AddValuesResult {Result = query.A + query.B};
            return Task.FromResult(result);
        }

        public Task<SubValuesResult> Handle(SubValues query, CancellationToken cancellationToken)
        {
            var result = new SubValuesResult { Result = query.A - query.B };
            return Task.FromResult(result);
        }
    }
}
