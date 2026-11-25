using Pid.Domain.Simulations;

namespace Pid.Application.Abstractions;

/// <summary>Storage for simulations. Application defines it, Infrastructure implements it.</summary>
public interface ISimulationRepository
{
    Task<Simulation?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Simulation simulation, CancellationToken cancellationToken = default);
    Task UpdateAsync(Simulation simulation, CancellationToken cancellationToken = default);
}
