using Pid.Application.Common;
using Pid.Application.Simulations.Commands.AdvanceSimulation;
using Pid.Application.Simulations.Commands.CreateSimulation;
using Pid.Application.Simulations.Queries.GetSimulationHistory;
using Pid.Application.Simulations.Queries.GetSimulationState;

namespace Pid.Application.Tests;

public class QueryHandlerTests
{
    private readonly FakeSimulationRepository _repository = new();
    private readonly Guid _id = Guid.NewGuid();

    [Fact]
    public async Task History_is_empty_before_the_simulation_runs()
    {
        await new CreateSimulationHandler(_repository).HandleAsync(TestData.CreateCommand(_id));

        var history = await new GetSimulationHistoryHandler(_repository)
            .HandleAsync(new GetSimulationHistoryQuery(_id));

        Assert.Empty(history);
    }

    [Fact]
    public async Task History_returns_newest_samples_oldest_first()
    {
        await new CreateSimulationHandler(_repository).HandleAsync(TestData.CreateCommand(_id));
        await new AdvanceSimulationHandler(_repository)
            .HandleAsync(new AdvanceSimulationCommand(_id, TimeSpan.FromSeconds(100)));

        var history = await new GetSimulationHistoryHandler(_repository)
            .HandleAsync(new GetSimulationHistoryQuery(_id, MaxSamples: 3));

        Assert.Equal([80, 90, 100], history.Select(s => s.TimeSeconds));
    }

    [Fact]
    public async Task State_of_unknown_simulation_throws_not_found()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            new GetSimulationStateHandler(_repository).HandleAsync(new GetSimulationStateQuery(Guid.NewGuid())));
    }
}
