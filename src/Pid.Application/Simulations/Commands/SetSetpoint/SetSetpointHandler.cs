using Pid.Application.Abstractions;
using Pid.Application.Common;

namespace Pid.Application.Simulations.Commands.SetSetpoint;

public sealed class SetSetpointHandler(ISimulationRepository repository) : ICommandHandler<SetSetpointCommand>
{
    public async Task HandleAsync(SetSetpointCommand command, CancellationToken cancellationToken = default)
    {
        var simulation = await repository.GetRequiredAsync(command.SimulationId, cancellationToken);

        simulation.ChangeSetpoint(command.SetpointC);

        await repository.UpdateAsync(simulation, cancellationToken);
    }
}
