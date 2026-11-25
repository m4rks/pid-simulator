using Pid.Application.Abstractions;
using Pid.Application.Simulations.Dtos;

namespace Pid.Application.Simulations.Queries.GetSimulationState;

public sealed record GetSimulationStateQuery(Guid SimulationId) : IQuery<SimulationStateDto>;
