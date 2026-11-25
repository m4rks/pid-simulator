using Pid.Application.Common;
using Pid.Application.Simulations.Commands.AdvanceSimulation;
using Pid.Application.Simulations.Commands.CreateSimulation;
using Pid.Application.Simulations.Commands.SetAmbientTemperature;
using Pid.Application.Simulations.Commands.SetSetpoint;
using Pid.Application.Simulations.Commands.UpdatePidParameters;
using Pid.Application.Simulations.Dtos;
using Pid.Application.Simulations.Queries.GetSimulationState;

namespace Pid.Application.Tests;

public class CommandHandlerTests
{
    private readonly FakeSimulationRepository _repository = new();
    private readonly Guid _id = Guid.NewGuid();

    [Fact]
    public async Task Create_stores_simulation_with_given_parameters()
    {
        await CreateSimulation();

        var state = await GetState();

        Assert.Equal(_id, state.Id);
        Assert.Equal(15, state.TemperatureC);
        Assert.Equal(21, state.SetpointC);
        Assert.Equal(0, state.AmbientC);
        Assert.Equal(10000, state.HeaterMaxPowerW);
        Assert.Equal(new HouseParametersDto(2e7, 0.005), state.House);
        Assert.Equal(new PidParametersDto(0.5, 0.5 / 3600, 0), state.Pid);
    }

    [Fact]
    public async Task SetSetpoint_changes_setpoint()
    {
        await CreateSimulation();

        await new SetSetpointHandler(_repository).HandleAsync(new SetSetpointCommand(_id, 19));

        Assert.Equal(19, (await GetState()).SetpointC);
        Assert.Equal(1, _repository.UpdateCount);
    }

    [Fact]
    public async Task SetAmbientTemperature_changes_ambient()
    {
        await CreateSimulation();

        await new SetAmbientTemperatureHandler(_repository).HandleAsync(new SetAmbientTemperatureCommand(_id, -15));

        Assert.Equal(-15, (await GetState()).AmbientC);
    }

    [Fact]
    public async Task UpdatePidParameters_changes_gains()
    {
        await CreateSimulation();
        var newPid = new PidParametersDto(Kp: 1, Ki: 0.001, Kd: 30);

        await new UpdatePidParametersHandler(_repository).HandleAsync(new UpdatePidParametersCommand(_id, newPid));

        Assert.Equal(newPid, (await GetState()).Pid);
    }

    [Fact]
    public async Task AdvanceSimulation_moves_time_and_heats_the_house()
    {
        await CreateSimulation();

        await new AdvanceSimulationHandler(_repository)
            .HandleAsync(new AdvanceSimulationCommand(_id, TimeSpan.FromHours(1)));

        var state = await GetState();
        Assert.Equal(3600, state.TimeSeconds);
        Assert.True(state.TemperatureC > 15);
        Assert.Equal(10000, state.HeaterPowerW); // still far from the setpoint, so full power
    }

    [Fact]
    public async Task Commands_on_unknown_simulation_throw_not_found()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            new SetSetpointHandler(_repository).HandleAsync(new SetSetpointCommand(Guid.NewGuid(), 20)));
    }

    private Task CreateSimulation() =>
        new CreateSimulationHandler(_repository).HandleAsync(TestData.CreateCommand(_id));

    private Task<SimulationStateDto> GetState() =>
        new GetSimulationStateHandler(_repository).HandleAsync(new GetSimulationStateQuery(_id));
}
