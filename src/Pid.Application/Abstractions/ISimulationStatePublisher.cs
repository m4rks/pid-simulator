using Pid.Application.Simulations.Dtos;

namespace Pid.Application.Abstractions;

/// <summary>Pushes live state to connected clients. Implemented in Api with SignalR.</summary>
public interface ISimulationStatePublisher
{
    Task PublishAsync(SimulationStateDto state, CancellationToken cancellationToken = default);
}
