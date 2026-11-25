namespace Pid.Application.Simulations.Dtos;

/// <param name="House">Set when the plant is a house; other plant types will get their own property.</param>
/// <param name="Pid">Set when the controller is a PID; other controller types will get their own property.</param>
public sealed record SimulationStateDto(
    Guid Id,
    double TimeSeconds,
    double TemperatureC,
    double SetpointC,
    double AmbientC,
    double ControlSignal,
    double HeaterPowerW,
    double HeaterMaxPowerW,
    HouseParametersDto? House,
    PidParametersDto? Pid);
