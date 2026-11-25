using Pid.Application.Abstractions;
using Pid.Application.Simulations.Dtos;
using Pid.Domain.Controllers;
using Pid.Domain.Heaters;
using Pid.Domain.Plants;
using Pid.Domain.Simulations;

namespace Pid.Application.Simulations.Commands.CreateSimulation;

public sealed class CreateSimulationHandler(ISimulationRepository repository)
    : ICommandHandler<CreateSimulationCommand>
{
    public async Task HandleAsync(CreateSimulationCommand command, CancellationToken cancellationToken = default)
    {
        var simulation = new Simulation(
            command.SimulationId,
            new HouseModel(command.House.ToDomain(), command.InitialTemperatureC),
            new PidController(command.Pid.ToDomain()),
            new Heater(command.HeaterMaxPowerW),
            command.SetpointC,
            command.AmbientC);

        await repository.AddAsync(simulation, cancellationToken);
    }
}
