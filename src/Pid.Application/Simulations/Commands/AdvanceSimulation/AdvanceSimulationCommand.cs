using Pid.Application.Abstractions;

namespace Pid.Application.Simulations.Commands.AdvanceSimulation;

/// <summary>
/// Moves simulated time forward. The background runner will send it periodically;
/// it can also be used for "fast forward" from the UI.
/// </summary>
public sealed record AdvanceSimulationCommand(Guid SimulationId, TimeSpan Duration) : ICommand;
