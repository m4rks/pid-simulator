using Pid.Domain.Common;
using Pid.Domain.Heaters;

namespace Pid.Domain.Tests;

public class HeaterTests
{
    [Theory]
    [InlineData(0.0, 0)]
    [InlineData(0.5, 5000)]
    [InlineData(1.0, 10000)]
    [InlineData(1.7, 10000)]
    [InlineData(-0.3, 0)]
    public void Converts_signal_to_power_within_limits(double signal, double expectedPowerW)
    {
        var heater = new Heater(maxPowerW: 10000);

        Assert.Equal(expectedPowerW, heater.GetPowerW(signal));
    }

    [Fact]
    public void Rejects_non_positive_power()
    {
        Assert.Throws<DomainException>(() => new Heater(0));
    }
}
