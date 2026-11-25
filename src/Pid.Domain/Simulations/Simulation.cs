using Pid.Domain.Controllers;
using Pid.Domain.Heaters;
using Pid.Domain.Plants;

namespace Pid.Domain.Simulations;

/// <summary>
/// Aggregate root: the closed control loop measurement -> controller -> heater -> plant.
/// </summary>
public sealed class Simulation(
    IThermalPlant plant,
    IController controller,
    Heater heater,
    double setpointC,
    double ambientC)
{
    public IThermalPlant Plant { get; } = plant;
    public IController Controller { get; } = controller;
    public Heater Heater { get; } = heater;
    public double SetpointC { get; private set; } = setpointC;
    public double AmbientC { get; private set; } = ambientC;
    public TimeSpan Time { get; private set; } = TimeSpan.Zero;

    public SimulationSample Tick(TimeSpan dt)
    {
        // The signal is computed from the current measurement and held constant for the whole step.
        var controlSignal = Controller.Compute(SetpointC, Plant.TemperatureC, dt);
        var heaterPowerW = Heater.GetPowerW(controlSignal);
        var temperatureC = Plant.Step(heaterPowerW, AmbientC, dt);
        Time += dt;

        return new SimulationSample(Time, temperatureC, SetpointC, AmbientC, controlSignal, heaterPowerW);
    }

    public void ChangeSetpoint(double setpointC) => SetpointC = setpointC;

    public void ChangeAmbient(double ambientC) => AmbientC = ambientC;
}
