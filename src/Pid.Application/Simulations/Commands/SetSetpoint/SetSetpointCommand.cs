using Pid.Application.Abstractions;

namespace Pid.Application.Simulations.Commands.SetSetpoint;

public sealed record SetSetpointCommand(Guid SimulationId, double SetpointC) : ICommand;
