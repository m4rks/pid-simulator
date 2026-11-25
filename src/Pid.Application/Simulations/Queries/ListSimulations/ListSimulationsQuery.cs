using Pid.Application.Abstractions;

namespace Pid.Application.Simulations.Queries.ListSimulations;

public sealed record ListSimulationsQuery : IQuery<IReadOnlyList<Guid>>;
