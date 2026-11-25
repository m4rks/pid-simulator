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

    private static Simulation CreateSimulation(double initialTemperatureC, double setpointC, double ambientC)
    {
        var house = new HouseModel(new HouseParameters(heatCapacityJPerK: 2e7, thermalResistanceKPerW: 0.005), initialTemperatureC);
        // PI controller: Kp = 0.5 (full power at 2 K error), integral time Kp/Ki = 1 h.
        var pid = new PidController(new PidParameters(kp: 0.5, ki: 0.5 / 3600, kd: 0));
        var heater = new Heater(maxPowerW: 10000);

        return new Simulation(house, pid, heater, setpointC, ambientC);
    }

    private static List<SimulationSample> RunFor(Simulation simulation, TimeSpan duration)
    {
        var samples = new List<SimulationSample>();
        for (var t = TimeSpan.Zero; t < duration; t += Dt)
            samples.Add(simulation.Tick(Dt));
        return samples;
    }
}
