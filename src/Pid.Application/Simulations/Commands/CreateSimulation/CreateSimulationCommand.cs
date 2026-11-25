using Pid.Application.Abstractions;
using Pid.Application.Simulations.Dtos;

namespace Pid.Application.Simulations.Commands.CreateSimulation;

/// <param name="SimulationId">Chosen by the caller, so the command does not need to return anything.</param>
public sealed record CreateSimulationCommand(
    Guid SimulationId,
    HouseParametersDto House,
    double InitialTemperatureC,
    double HeaterMaxPowerW,
    PidParametersDto Pid,
    double SetpointC,
    double AmbientC) : ICommand;
