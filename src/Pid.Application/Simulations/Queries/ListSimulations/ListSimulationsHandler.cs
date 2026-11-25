using Pid.Application.Abstractions;

namespace Pid.Application.Simulations.Queries.ListSimulations;

public sealed class ListSimulationsHandler(ISimulationRepository repository)
    : IQueryHandler<ListSimulationsQuery, IReadOnlyList<Guid>>
{
    public Task<IReadOnlyList<Guid>> HandleAsync(ListSimulationsQuery query, CancellationToken cancellationToken = default) =>
        repository.ListIdsAsync(cancellationToken);
}
