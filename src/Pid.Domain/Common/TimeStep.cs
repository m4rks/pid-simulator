namespace Pid.Domain.Common;

internal static class TimeStep
{
    public static double ToPositiveSeconds(TimeSpan dt) =>
        dt > TimeSpan.Zero ? dt.TotalSeconds : throw new DomainException("Time step must be positive.");
}
