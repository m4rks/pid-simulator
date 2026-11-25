using System.Collections.Concurrent;
using Pid.Application.Abstractions;
using Pid.Domain.Simulations;

namespace Pid.Infrastructure.Persistence;

/// <summary>Keeps simulations in memory; they disappear when the application stops.</summary>
internal sealed class InMemorySimulationRepository : ISimulationRepository
{
    private readonly ConcurrentDictionary<Guid, Simulation> _simulations = new();

    public Task<Simulation?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_simulations.GetValueOrDefault(id));

    public Task<IReadOnlyList<Guid>> ListIdsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Guid>>([.. _simulations.Keys]);

    public Task AddAsync(Simulation simulation, CancellationToken cancellationToken = default)
    {
        if (!_simulations.TryAdd(simulation.Id, simulation))
            throw new InvalidOperationException($"Simulation '{simulation.Id}' already exists.");

        return Task.CompletedTask;
    }

    // The stored object is the one the handler changed, so there is nothing to write back.
    // A database implementation would save the changes here.
    public Task UpdateAsync(Simulation simulation, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
