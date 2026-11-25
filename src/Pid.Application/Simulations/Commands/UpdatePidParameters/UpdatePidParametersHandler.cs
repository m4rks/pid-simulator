using Pid.Application.Abstractions;
using Pid.Application.Common;
using Pid.Application.Simulations.Dtos;

namespace Pid.Application.Simulations.Commands.UpdatePidParameters;

public sealed class UpdatePidParametersHandler(ISimulationRepository repository)
    : ICommandHandler<UpdatePidParametersCommand>
{
    public async Task HandleAsync(UpdatePidParametersCommand command, CancellationToken cancellationToken = default)
    {
        var simulation = await repository.GetRequiredAsync(command.SimulationId, cancellationToken);

        simulation.UpdatePidParameters(command.Pid.ToDomain());

        await repository.UpdateAsync(simulation, cancellationToken);
    }
}
