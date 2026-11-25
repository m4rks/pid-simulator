namespace Pid.Application.Simulations.Dtos;

/// <summary>One point on the charts. Time is in seconds, which is easy to plot in the frontend.</summary>
public sealed record SimulationSampleDto(
    double TimeSeconds,
    double TemperatureC,
    double SetpointC,
    double AmbientC,
    double ControlSignal,
    double HeaterPowerW);
