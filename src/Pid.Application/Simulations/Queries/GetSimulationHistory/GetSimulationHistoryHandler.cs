using Pid.Application.Abstractions;
using Pid.Application.Common;
using Pid.Application.Simulations.Dtos;

namespace Pid.Application.Simulations.Queries.GetSimulationHistory;

public sealed class GetSimulationHistoryHandler(ISimulationRepository repository)
    : IQueryHandler<GetSimulationHistoryQuery, IReadOnlyList<SimulationSampleDto>>
{
    public async Task<IReadOnlyList<SimulationSampleDto>> HandleAsync(
        GetSimulationHistoryQuery query, CancellationToken cancellationToken = default)
    {
        var simulation = await repository.GetRequiredAsync(query.SimulationId, cancellationToken);

        return simulation.History
            .TakeLast(query.MaxSamples)
            .Select(sample => sample.ToDto())
            .ToList();
    }
}
