using Pid.Application.Abstractions;
using Pid.Domain.Simulations;

namespace Pid.Application.Common;

internal static class SimulationRepositoryExtensions
{
    public static async Task<Simulation> GetRequiredAsync(
        this ISimulationRepository repository, Guid id, CancellationToken cancellationToken) =>
        await repository.GetAsync(id, cancellationToken)
        ?? throw new NotFoundException($"Simulation '{id}' was not found.");
}
