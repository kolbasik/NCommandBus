using System.Threading;
using System.Threading.Tasks;
using kolbasik.NCommandBus.Abstractions;
using Sample.Commands;

namespace Sample.Handles
{
    public sealed class Calculator : IQueryHandler<GetAddedValues, GetAddedValuesResult>, IQueryHandler<GetSubtractedValues, GetSubtractedValuesResult>
    {
        public Task<GetAddedValuesResult> Handle(GetAddedValues query, CancellationToken cancellationToken)
        {
            var result = new GetAddedValuesResult { Result = query.A + query.B};
            return Task.FromResult(result);
        }

        public Task<GetSubtractedValuesResult> Handle(GetSubtractedValues query, CancellationToken cancellationToken)
        {
            var result = new GetSubtractedValuesResult { Result = query.A - query.B };
            return Task.FromResult(result);
        }
    }
}
