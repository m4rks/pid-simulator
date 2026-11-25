using Pid.Domain.Common;
using Pid.Domain.Controllers;

namespace Pid.Domain.Tests;

public class PidControllerTests
{
    private static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);

    [Fact]
    public void Proportional_term_is_gain_times_error()
    {
        var pid = new PidController(new PidParameters(kp: 0.1, ki: 0, kd: 0));

        var output = pid.Compute(setpointC: 21, measuredC: 18, OneSecond);

        Assert.Equal(0.3, output, precision: 9);
    }

    [Fact]
    public void Integral_term_grows_while_error_persists()
    {
        var pid = new PidController(new PidParameters(kp: 0, ki: 0.1, kd: 0));

        var first = pid.Compute(setpointC: 21, measuredC: 20, OneSecond);
        var second = pid.Compute(setpointC: 21, measuredC: 20, OneSecond);

        Assert.Equal(0.1, first, precision: 9);
        Assert.Equal(0.2, second, precision: 9);
    }

    [Fact]
    public void Derivative_term_pushes_output_up_when_temperature_falls()
    {
        var pid = new PidController(new PidParameters(kp: 0, ki: 0, kd: 1));
        pid.Compute(setpointC: 21, measuredC: 20, OneSecond);

        // Temperature falls by 0.5 K/s -> derivative pushes the output up by 1 * 0.5.
        var output = pid.Compute(setpointC: 21, measuredC: 19.5, OneSecond);

        Assert.Equal(0.5, output, precision: 9);
    }

    [Fact]
    public void Setpoint_change_does_not_cause_derivative_kick()
    {
        var pid = new PidController(new PidParameters(kp: 0, ki: 0, kd: 100));
        pid.Compute(setpointC: 21, measuredC: 20, OneSecond);

        var output = pid.Compute(setpointC: 30, measuredC: 20, OneSecond);

        Assert.Equal(0, output);
    }

    [Fact]
    public void Output_is_limited_to_0_1()
    {
        var pid = new PidController(new PidParameters(kp: 10, ki: 0, kd: 0));

        Assert.Equal(1, pid.Compute(setpointC: 30, measuredC: 0, OneSecond));
        Assert.Equal(0, pid.Compute(setpointC: 0, measuredC: 30, OneSecond));
    }

    [Fact]
    public void Integral_does_not_wind_up_while_output_is_saturated()
    {
        var pid = new PidController(new PidParameters(kp: 1, ki: 0.01, kd: 0));

        // A long time at full power with a large error...
        for (var i = 0; i < 1000; i++)
            pid.Compute(setpointC: 30, measuredC: 20, OneSecond);

        // ...then a small overshoot. Without anti-windup the integral would be ~100 and the output still 1.
        var output = pid.Compute(setpointC: 30, measuredC: 30.5, OneSecond);

        Assert.Equal(0, output);
    }

    [Fact]
    public void Reset_clears_integral()
    {
        var pid = new PidController(new PidParameters(kp: 0, ki: 0.1, kd: 0));
        pid.Compute(setpointC: 21, measuredC: 20, OneSecond);

        pid.Reset();

        Assert.Equal(0.1, pid.Compute(setpointC: 21, measuredC: 20, OneSecond), precision: 9);
    }

    [Fact]
    public void Rejects_negative_gains()
    {
        Assert.Throws<DomainException>(() => new PidParameters(kp: -1, ki: 0, kd: 0));
    }

    [Fact]
    public void Rejects_non_positive_time_step()
    {
        var pid = new PidController(new PidParameters(kp: 1, ki: 0, kd: 0));

        Assert.Throws<DomainException>(() => pid.Compute(21, 20, TimeSpan.Zero));
    }
}
