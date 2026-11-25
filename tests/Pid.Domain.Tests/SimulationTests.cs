using Pid.Domain.Common;
using Pid.Domain.Controllers;
using Pid.Domain.Heaters;
using Pid.Domain.Plants;
using Pid.Domain.Simulations;

namespace Pid.Domain.Tests;

public class SimulationTests
{
    private static readonly TimeSpan Dt = TimeSpan.FromSeconds(10);

    [Fact]
    public void House_reaches_setpoint()
    {
        var simulation = CreateSimulation(initialTemperatureC: 15, setpointC: 21, ambientC: 0);

        var samples = RunFor(simulation, TimeSpan.FromHours(48));

        Assert.Equal(21, samples[^1].TemperatureC, precision: 1);
        // In steady state the heater exactly covers heat loss: 21 K / 0.005 K/W = 4200 W.
        Assert.Equal(4200, samples[^1].HeaterPowerW, precision: 0);
    }

    [Fact]
    public void Does_not_overshoot_much()
    {
        var simulation = CreateSimulation(initialTemperatureC: 15, setpointC: 21, ambientC: 0);

        var samples = RunFor(simulation, TimeSpan.FromHours(48));

        Assert.True(samples.Max(s => s.TemperatureC) < 21.5);
    }

    [Fact]
    public void Returns_to_setpoint_after_ambient_drops()
    {
        var simulation = CreateSimulation(initialTemperatureC: 21, setpointC: 21, ambientC: 0);
        RunFor(simulation, TimeSpan.FromHours(48));

        simulation.ChangeAmbient(-10);
        var samples = RunFor(simulation, TimeSpan.FromHours(48));

        Assert.True(samples.Min(s => s.TemperatureC) < 21, "the frost should cool the house for a moment");
        Assert.Equal(21, samples[^1].TemperatureC, precision: 1);
    }

    [Fact]
    public void Follows_setpoint_change()
    {
        var simulation = CreateSimulation(initialTemperatureC: 21, setpointC: 21, ambientC: 0);
        RunFor(simulation, TimeSpan.FromHours(48));

        simulation.ChangeSetpoint(18);
        var samples = RunFor(simulation, TimeSpan.FromHours(96));

        Assert.Equal(18, samples[^1].TemperatureC, precision: 1);
    }

    [Fact]
    public void Tick_advances_time()
    {
        var simulation = CreateSimulation(initialTemperatureC: 15, setpointC: 21, ambientC: 0);

        simulation.Tick(Dt);
        var sample = simulation.Tick(Dt);

        Assert.Equal(TimeSpan.FromSeconds(20), sample.Time);
    }

    [Fact]
    public void Advance_runs_ticks_of_step_size_and_records_history()
    {
        var simulation = CreateSimulation(initialTemperatureC: 15, setpointC: 21, ambientC: 0);

        simulation.Advance(TimeSpan.FromSeconds(25));

        Assert.Equal(TimeSpan.FromSeconds(25), simulation.Time);
        Assert.Equal(
            [TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(25)],
            simulation.History.Select(s => s.Time));
    }

    [Fact]
    public void History_keeps_only_the_newest_samples()
    {
        var simulation = CreateSimulation(initialTemperatureC: 15, setpointC: 21, ambientC: 0);

        simulation.Advance(Simulation.StepSize * (Simulation.MaxHistorySamples + 5));

        Assert.Equal(Simulation.MaxHistorySamples, simulation.History.Count);
        Assert.Equal(simulation.Time, simulation.History.Last().Time);
    }

    [Fact]
    public void Pid_parameters_can_be_updated()
    {
        var simulation = CreateSimulation(initialTemperatureC: 15, setpointC: 21, ambientC: 0);
        var newParameters = new PidParameters(kp: 1, ki: 0.001, kd: 10);

        simulation.UpdatePidParameters(newParameters);

        Assert.Equal(newParameters, ((PidController)simulation.Controller).Parameters);
    }

    [Fact]
    public void Rejects_empty_id()
    {
        var house = new HouseModel(new HouseParameters(2e7, 0.005), 15);
        var pid = new PidController(new PidParameters(0.5, 0, 0));

        Assert.Throws<DomainException>(() => new Simulation(Guid.Empty, house, pid, new Heater(10000), 21, 0));
    }

    private static Simulation CreateSimulation(double initialTemperatureC, double setpointC, double ambientC)
    {
        var house = new HouseModel(new HouseParameters(heatCapacityJPerK: 2e7, thermalResistanceKPerW: 0.005), initialTemperatureC);
        // PI controller: Kp = 0.5 (full power at 2 K error), integral time Kp/Ki = 1 h.
        var pid = new PidController(new PidParameters(kp: 0.5, ki: 0.5 / 3600, kd: 0));
        var heater = new Heater(maxPowerW: 10000);

        return new Simulation(Guid.NewGuid(), house, pid, heater, setpointC, ambientC);
    }

    private static List<SimulationSample> RunFor(Simulation simulation, TimeSpan duration)
    {
        var samples = new List<SimulationSample>();
        for (var t = TimeSpan.Zero; t < duration; t += Dt)
            samples.Add(simulation.Tick(Dt));
        return samples;
    }
}
