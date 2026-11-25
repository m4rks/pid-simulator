namespace Pid.Domain.Simulations;

/// <summary>State of the control loop at a single point in time (one point on the charts).</summary>
public sealed record SimulationSample(
    TimeSpan Time,
    double TemperatureC,
    double SetpointC,
    double AmbientC,
    double ControlSignal,
    double HeaterPowerW);
