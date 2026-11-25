using Pid.Application.Simulations.Commands.AdvanceSimulation;
using Pid.Application.Simulations.Commands.CreateSimulation;
using Pid.Application.Simulations.Commands.SetAmbientTemperature;
using Pid.Application.Simulations.Commands.SetSetpoint;
using Pid.Application.Simulations.Commands.UpdatePidParameters;
using Pid.Application.Simulations.Dtos;

namespace Pid.Application.Tests;

public class ValidatorTests
{
    private static readonly Guid Id = Guid.NewGuid();

    [Fact]
    public void Valid_create_command_passes()
    {
        Assert.True(new CreateSimulationValidator().Validate(TestData.CreateCommand()).IsValid);
    }

    [Fact]
    public void Create_rejects_invalid_nested_parameters()
    {
        var command = TestData.CreateCommand() with
        {
            House = new HouseParametersDto(HeatCapacityJPerK: 0, ThermalResistanceKPerW: 0.005),
            Pid = new PidParametersDto(Kp: -1, Ki: 0, Kd: 0),
        };

        var result = new CreateSimulationValidator().Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == "House.HeatCapacityJPerK");
        Assert.Contains(result.Errors, e => e.PropertyName == "Pid.Kp");
    }

    [Theory]
    [InlineData(4.9, false)]
    [InlineData(5, true)]
    [InlineData(21, true)]
    [InlineData(35, true)]
    [InlineData(35.1, false)]
    [InlineData(double.NaN, false)]
    public void Setpoint_must_be_within_limits(double setpointC, bool expectedValid)
    {
        var result = new SetSetpointValidator().Validate(new SetSetpointCommand(Id, setpointC));

        Assert.Equal(expectedValid, result.IsValid);
    }

    [Theory]
    [InlineData(-40, true)]
    [InlineData(-41, false)]
    [InlineData(45, true)]
    [InlineData(46, false)]
    public void Ambient_must_be_within_limits(double ambientC, bool expectedValid)
    {
        var result = new SetAmbientTemperatureValidator().Validate(new SetAmbientTemperatureCommand(Id, ambientC));

        Assert.Equal(expectedValid, result.IsValid);
    }

    [Theory]
    [InlineData(0, 0, 0, true)]
    [InlineData(-0.1, 0, 0, false)]
    [InlineData(1, double.PositiveInfinity, 0, false)]
    [InlineData(1, 0, double.NaN, false)]
    public void Pid_gains_must_be_finite_and_non_negative(double kp, double ki, double kd, bool expectedValid)
    {
        var command = new UpdatePidParametersCommand(Id, new PidParametersDto(kp, ki, kd));

        Assert.Equal(expectedValid, new UpdatePidParametersValidator().Validate(command).IsValid);
    }

    [Fact]
    public void Empty_simulation_id_is_rejected()
    {
        Assert.False(new SetSetpointValidator().Validate(new SetSetpointCommand(Guid.Empty, 21)).IsValid);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(60, true)]
    [InlineData(8 * 24 * 3600, false)]
    public void Advance_duration_must_be_positive_and_limited(int seconds, bool expectedValid)
    {
        var command = new AdvanceSimulationCommand(Id, TimeSpan.FromSeconds(seconds));

        Assert.Equal(expectedValid, new AdvanceSimulationValidator().Validate(command).IsValid);
    }
}
