using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pid.Application.Abstractions;
using Pid.Application.Simulations.Commands.AdvanceSimulation;
using Pid.Application.Simulations.Dtos;
using Pid.Application.Simulations.Queries.GetSimulationState;
using Pid.Application.Simulations.Queries.ListSimulations;

namespace Pid.Infrastructure.Runner;

/// <summary>
/// Background loop: on every tick advances each simulation and publishes its new state.
/// It goes through the same command and query handlers as the API, never touching the domain directly.
/// </summary>
internal sealed class SimulationRunner(
    IServiceScopeFactory scopeFactory,
    IOptions<SimulationRunnerOptions> options,
    ILogger<SimulationRunner> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(options.Value.TickInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await AdvanceAllAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // One failed tick must not stop the loop for good.
                logger.LogError(ex, "Simulation tick failed.");
            }
        }
    }

    private async Task AdvanceAllAsync(CancellationToken cancellationToken)
    {
        // Handlers are scoped, and a BackgroundService lives for the whole application, so it creates a scope per tick.
        await using var scope = scopeFactory.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var list = services.GetRequiredService<IQueryHandler<ListSimulationsQuery, IReadOnlyList<Guid>>>();
        var advance = services.GetRequiredService<ICommandHandler<AdvanceSimulationCommand>>();
        var getState = services.GetRequiredService<IQueryHandler<GetSimulationStateQuery, SimulationStateDto>>();
        var publisher = services.GetRequiredService<ISimulationStatePublisher>();

        foreach (var id in await list.HandleAsync(new ListSimulationsQuery(), cancellationToken))
        {
            await advance.HandleAsync(new AdvanceSimulationCommand(id, options.Value.SimulatedTimePerTick), cancellationToken);
            var state = await getState.HandleAsync(new GetSimulationStateQuery(id), cancellationToken);
            await publisher.PublishAsync(state, cancellationToken);
        }
    }
}
