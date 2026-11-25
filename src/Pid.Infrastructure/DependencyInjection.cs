using Microsoft.Extensions.DependencyInjection;
using Pid.Application.Abstractions;
using Pid.Infrastructure.Persistence;
using Pid.Infrastructure.Runner;

namespace Pid.Infrastructure;

public static class DependencyInjection
{
    /// <summary>The caller must also register <see cref="ISimulationStatePublisher"/>.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Singleton: one shared store for the whole application lifetime.
        services.AddSingleton<ISimulationRepository, InMemorySimulationRepository>();

        services.AddOptions<SimulationRunnerOptions>().BindConfiguration(SimulationRunnerOptions.SectionName);
        services.AddHostedService<SimulationRunner>();

        return services;
    }
}
