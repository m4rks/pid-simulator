using Pid.Application.Abstractions;

namespace Pid.Application.Simulations.Commands.SetAmbientTemperature;

public sealed record SetAmbientTemperatureCommand(Guid SimulationId, double AmbientC) : ICommand;
