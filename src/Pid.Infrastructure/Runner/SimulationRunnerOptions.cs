namespace Pid.Infrastructure.Runner;

/// <summary>
/// A house reacts over hours, so the runner speeds time up:
/// by default each 250 ms of real time moves the simulation 2 minutes forward (480x).
/// </summary>
public sealed class SimulationRunnerOptions
{
    public const string SectionName = "SimulationRunner";

    public TimeSpan TickInterval { get; set; } = TimeSpan.FromMilliseconds(250);
    public TimeSpan SimulatedTimePerTick { get; set; } = TimeSpan.FromMinutes(2);
}
