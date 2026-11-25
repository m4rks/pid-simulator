using Pid.Application.Abstractions;
using Pid.Application.Simulations.Dtos;

namespace Pid.Application.Simulations.Queries.GetSimulationHistory;

/// <param name="MaxSamples">Returns only the newest samples, oldest first.</param>
public sealed record GetSimulationHistoryQuery(Guid SimulationId, int MaxSamples = 1000)
    : IQuery<IReadOnlyList<SimulationSampleDto>>;
