using Pid.Application.Abstractions;
using Pid.Application.Common;
using Pid.Application.Simulations.Dtos;

namespace Pid.Application.Simulations.Queries.GetSimulationState;

public sealed class GetSimulationStateHandler(ISimulationRepository repository)
    : IQueryHandler<GetSimulationStateQuery, SimulationStateDto>
{
    public async Task<SimulationStateDto> HandleAsync(
        GetSimulationStateQuery query, CancellationToken cancellationToken = default)
    {
        var simulation = await repository.GetRequiredAsync(query.SimulationId, cancellationToken);

        return simulation.ToStateDto();
    }
}
