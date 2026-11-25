using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Pid.Application.Abstractions;
using Pid.Application.Simulations.Commands.CreateSimulation;
using Pid.Application.Simulations.Commands.SetSetpoint;
using Pid.Application.Simulations.Dtos;
using Pid.Application.Simulations.Queries.GetSimulationHistory;
using Pid.Application.Simulations.Queries.GetSimulationState;

namespace Pid.Application.Tests;

/// <summary>Checks the wiring the API will use: handlers resolved from DI, wrapped in validation.</summary>
public class DependencyInjectionTests
{
    private readonly ServiceProvider _provider = new ServiceCollection()
        .AddApplication()
        .AddSingleton<ISimulationRepository, FakeSimulationRepository>()
        .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });

    [Fact]
    public void All_handlers_can_be_resolved()
    {
        using var scope = _provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateSimulationCommand>>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IQueryHandler<GetSimulationStateQuery, SimulationStateDto>>());
        Assert.NotNull(scope.ServiceProvider
            .GetRequiredService<IQueryHandler<GetSimulationHistoryQuery, IReadOnlyList<SimulationSampleDto>>>());
    }

    [Fact]
    public async Task Invalid_command_is_rejected_before_reaching_the_handler()
    {
        using var scope = _provider.CreateScope();
        var services = scope.ServiceProvider;
        var id = Guid.NewGuid();
        await services.GetRequiredService<ICommandHandler<CreateSimulationCommand>>()
            .HandleAsync(TestData.CreateCommand(id));

        await Assert.ThrowsAsync<ValidationException>(() =>
            services.GetRequiredService<ICommandHandler<SetSetpointCommand>>()
                .HandleAsync(new SetSetpointCommand(id, SetpointC: 500)));

        var state = await services.GetRequiredService<IQueryHandler<GetSimulationStateQuery, SimulationStateDto>>()
            .HandleAsync(new GetSimulationStateQuery(id));
        Assert.Equal(21, state.SetpointC);
    }
}
