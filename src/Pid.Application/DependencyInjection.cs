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

namespace Pid.Application;

public static class DependencyInjection
{
    /// <summary>Registers all handlers and validators. The caller must also register <see cref="ISimulationRepository"/>.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddCommandHandler<CreateSimulationCommand, CreateSimulationHandler>();
        services.AddCommandHandler<SetSetpointCommand, SetSetpointHandler>();
        services.AddCommandHandler<SetAmbientTemperatureCommand, SetAmbientTemperatureHandler>();
        services.AddCommandHandler<UpdatePidParametersCommand, UpdatePidParametersHandler>();
        services.AddCommandHandler<AdvanceSimulationCommand, AdvanceSimulationHandler>();

        services.AddScoped<IQueryHandler<GetSimulationStateQuery, SimulationStateDto>, GetSimulationStateHandler>();
        services.AddScoped<
            IQueryHandler<GetSimulationHistoryQuery, IReadOnlyList<SimulationSampleDto>>,
            GetSimulationHistoryHandler>();

        return services;
    }

    /// <summary>Whoever asks for ICommandHandler&lt;TCommand&gt; gets the real handler wrapped in validation.</summary>
    private static void AddCommandHandler<TCommand, THandler>(this IServiceCollection services)
        where TCommand : ICommand
        where THandler : class, ICommandHandler<TCommand>
    {
        services.AddScoped<THandler>();
        services.AddScoped<ICommandHandler<TCommand>>(provider => new ValidatingCommandHandler<TCommand>(
            provider.GetRequiredService<THandler>(),
            provider.GetServices<IValidator<TCommand>>()));
    }
}
