using Pid.Application.Abstractions;
using Pid.Domain.Simulations;

namespace Pid.Application.Tests;

/// <summary>Test double that keeps simulations in a dictionary.</summary>
internal sealed class FakeSimulationRepository : ISimulationRepository
{
    private readonly Dictionary<Guid, Simulation> _simulations = [];

    public int UpdateCount { get; private set; }

    public Task<Simulation?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_simulations.GetValueOrDefault(id));

    public Task<IReadOnlyList<Guid>> ListIdsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Guid>>([.. _simulations.Keys]);

    public Task AddAsync(Simulation simulation, CancellationToken cancellationToken = default)
    {
        _simulations.Add(simulation.Id, simulation);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Simulation simulation, CancellationToken cancellationToken = default)
    {
        UpdateCount++;
        return Task.CompletedTask;
    }
}
