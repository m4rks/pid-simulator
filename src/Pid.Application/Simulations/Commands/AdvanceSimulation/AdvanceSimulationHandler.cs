using Pid.Application.Abstractions;
using Pid.Application.Common;

namespace Pid.Application.Simulations.Commands.AdvanceSimulation;

public sealed class AdvanceSimulationHandler(ISimulationRepository repository)
    : ICommandHandler<AdvanceSimulationCommand>
{
    public async Task HandleAsync(AdvanceSimulationCommand command, CancellationToken cancellationToken = default)
    {
        var simulation = await repository.GetRequiredAsync(command.SimulationId, cancellationToken);

        simulation.Advance(command.Duration);

        await repository.UpdateAsync(simulation, cancellationToken);
    }
}
