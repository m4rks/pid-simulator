using Pid.Application.Abstractions;
using Pid.Application.Simulations.Dtos;

namespace Pid.Application.Simulations.Commands.UpdatePidParameters;

public sealed record UpdatePidParametersCommand(Guid SimulationId, PidParametersDto Pid) : ICommand;
