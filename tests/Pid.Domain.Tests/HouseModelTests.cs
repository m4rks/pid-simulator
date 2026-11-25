using Pid.Domain.Common;
using Pid.Domain.Plants;

namespace Pid.Domain.Tests;

public class HouseModelTests
{
    // C = 20 MJ/K, R = 0.005 K/W -> time constant 100 000 s (~28 h), heat loss 200 W per 1 K of difference.
    private static readonly HouseParameters Parameters = new(heatCapacityJPerK: 2e7, thermalResistanceKPerW: 0.005);

    [Fact]
    public void Stays_constant_in_equilibrium()
    {
        var house = new HouseModel(Parameters, initialTemperatureC: 10);

        house.Step(heatPowerW: 0, ambientC: 10, TimeSpan.FromHours(1));

        Assert.Equal(10, house.TemperatureC, precision: 9);
    }

    [Fact]
    public void Cools_down_to_ambient_without_heating()
    {
        var house = new HouseModel(Parameters, initialTemperatureC: 20);

        RunFor(house, heatPowerW: 0, ambientC: 0, TimeSpan.FromDays(14));

        Assert.Equal(0, house.TemperatureC, precision: 2);
    }

    [Fact]
    public void Reaches_steady_state_of_ambient_plus_power_times_resistance()
    {
        var house = new HouseModel(Parameters, initialTemperatureC: 0);

        RunFor(house, heatPowerW: 4000, ambientC: 0, TimeSpan.FromDays(14));

        // 4000 W * 0.005 K/W = 20 K above ambient
        Assert.Equal(20, house.TemperatureC, precision: 2);
    }

    [Fact]
    public void After_one_time_constant_covers_63_percent_of_the_change()
    {
        var house = new HouseModel(Parameters, initialTemperatureC: 0);

        RunFor(house, heatPowerW: 4000, ambientC: 0, Parameters.TimeConstant);

        Assert.Equal(20 * (1 - Math.Exp(-1)), house.TemperatureC, precision: 6);
    }

    [Theory]
    [InlineData(0, 0.005)]
    [InlineData(2e7, -1)]
    public void Rejects_non_positive_parameters(double heatCapacity, double resistance)
    {
        Assert.Throws<DomainException>(() => new HouseParameters(heatCapacity, resistance));
    }

    private static void RunFor(HouseModel house, double heatPowerW, double ambientC, TimeSpan duration)
    {
        var dt = TimeSpan.FromSeconds(10);
        for (var t = TimeSpan.Zero; t < duration; t += dt)
            house.Step(heatPowerW, ambientC, dt);
    }
}
