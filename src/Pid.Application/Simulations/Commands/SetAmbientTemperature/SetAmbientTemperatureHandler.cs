using Pid.Application.Abstractions;
using Pid.Application.Common;

namespace Pid.Application.Simulations.Commands.SetAmbientTemperature;

public sealed class SetAmbientTemperatureHandler(ISimulationRepository repository)
    : ICommandHandler<SetAmbientTemperatureCommand>
{
    public async Task HandleAsync(SetAmbientTemperatureCommand command, CancellationToken cancellationToken = default)
    {
        var simulation = await repository.GetRequiredAsync(command.SimulationId, cancellationToken);

        simulation.ChangeAmbient(command.AmbientC);

        await repository.UpdateAsync(simulation, cancellationToken);
    }
}
