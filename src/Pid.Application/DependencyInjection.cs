using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Pid.Application.Abstractions;
using Pid.Application.Common;
using Pid.Application.Simulations.Commands.AdvanceSimulation;
using Pid.Application.Simulations.Commands.CreateSimulation;
using Pid.Application.Simulations.Commands.SetAmbientTemperature;
using Pid.Application.Simulations.Commands.SetSetpoint;
using Pid.Application.Simulations.Commands.UpdatePidParameters;
using Pid.Application.Simulations.Dtos;
using Pid.Application.Simulations.Queries.GetSimulationHistory;
using Pid.Application.Simulations.Queries.GetSimulationState;
using Pid.Application.Simulations.Queries.ListSimulations;

namespace Pid.Application;

public static class DependencyInjection
{
    /// <summary>Registers all handlers and validators. The caller must also register <see cref="ISimulationRepository"/>.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddSingleton<ExecutionGate>();

        services.AddCommandHandler<CreateSimulationCommand, CreateSimulationHandler>();
        services.AddCommandHandler<SetSetpointCommand, SetSetpointHandler>();
        services.AddCommandHandler<SetAmbientTemperatureCommand, SetAmbientTemperatureHandler>();
        services.AddCommandHandler<UpdatePidParametersCommand, UpdatePidParametersHandler>();
        services.AddCommandHandler<AdvanceSimulationCommand, AdvanceSimulationHandler>();

        services.AddQueryHandler<GetSimulationStateQuery, SimulationStateDto, GetSimulationStateHandler>();
        services.AddQueryHandler<GetSimulationHistoryQuery, IReadOnlyList<SimulationSampleDto>, GetSimulationHistoryHandler>();
        services.AddQueryHandler<ListSimulationsQuery, IReadOnlyList<Guid>, ListSimulationsHandler>();

        return services;
    }

    /// <summary>
    /// Whoever asks for ICommandHandler&lt;TCommand&gt; gets: validation -> one-at-a-time gate -> real handler.
    /// Validation runs outside the gate, so invalid commands never wait for it.
    /// </summary>
    private static void AddCommandHandler<TCommand, THandler>(this IServiceCollection services)
        where TCommand : ICommand
        where THandler : class, ICommandHandler<TCommand>
    {
        services.AddScoped<THandler>();
        services.AddScoped<ICommandHandler<TCommand>>(provider => new ValidatingCommandHandler<TCommand>(
            new SerializedCommandHandler<TCommand>(
                provider.GetRequiredService<THandler>(),
                provider.GetRequiredService<ExecutionGate>()),
            provider.GetServices<IValidator<TCommand>>()));
    }

    private static void AddQueryHandler<TQuery, TResult, THandler>(this IServiceCollection services)
        where TQuery : IQuery<TResult>
        where THandler : class, IQueryHandler<TQuery, TResult>
    {
        services.AddScoped<THandler>();
        services.AddScoped<IQueryHandler<TQuery, TResult>>(provider => new SerializedQueryHandler<TQuery, TResult>(
            provider.GetRequiredService<THandler>(),
            provider.GetRequiredService<ExecutionGate>()));
    }
}
