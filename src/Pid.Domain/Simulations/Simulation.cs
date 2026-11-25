using Pid.Domain.Common;
using Pid.Domain.Controllers;
using Pid.Domain.Heaters;
using Pid.Domain.Plants;

namespace Pid.Domain.Simulations;

/// <summary>
/// Aggregate root: the closed control loop measurement -> controller -> heater -> plant.
/// </summary>
public sealed class Simulation(
    Guid id,
    IThermalPlant plant,
    IController controller,
    Heater heater,
    double setpointC,
    double ambientC)
{
    public static readonly TimeSpan StepSize = TimeSpan.FromSeconds(10);

    // Keeps memory bounded during long runs (~55 h of simulated time at the default step).
    public const int MaxHistorySamples = 20_000;

    private readonly Queue<SimulationSample> _history = new();

    public Guid Id { get; } = id != Guid.Empty ? id : throw new DomainException("Simulation id cannot be empty.");
    public IThermalPlant Plant { get; } = plant;
    public IController Controller { get; } = controller;
    public Heater Heater { get; } = heater;
    public double SetpointC { get; private set; } = setpointC;
    public double AmbientC { get; private set; } = ambientC;
    public TimeSpan Time { get; private set; } = TimeSpan.Zero;

    /// <summary>Samples from oldest to newest.</summary>
    public IReadOnlyCollection<SimulationSample> History => _history;

    public SimulationSample Tick(TimeSpan dt)
    {
        // The signal is computed from the current measurement and held constant for the whole step.
        var controlSignal = Controller.Compute(SetpointC, Plant.TemperatureC, dt);
        var heaterPowerW = Heater.GetPowerW(controlSignal);
        var temperatureC = Plant.Step(heaterPowerW, AmbientC, dt);
        Time += dt;

        var sample = new SimulationSample(Time, temperatureC, SetpointC, AmbientC, controlSignal, heaterPowerW);
        _history.Enqueue(sample);
        if (_history.Count > MaxHistorySamples)
            _history.Dequeue();

        return sample;
    }

    /// <summary>Runs as many <see cref="StepSize"/> ticks as needed to move forward by <paramref name="duration"/>.</summary>
    public void Advance(TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
            throw new DomainException("Duration must be positive.");

        for (var remaining = duration; remaining > TimeSpan.Zero;)
        {
            var dt = remaining < StepSize ? remaining : StepSize;
            Tick(dt);
            remaining -= dt;
        }
    }

    public void ChangeSetpoint(double setpointC) => SetpointC = setpointC;

    public void ChangeAmbient(double ambientC) => AmbientC = ambientC;

    public void UpdatePidParameters(PidParameters parameters)
    {
        if (Controller is not PidController pid)
            throw new DomainException("The simulation is not controlled by a PID controller.");

        pid.UpdateParameters(parameters);
    }
}
