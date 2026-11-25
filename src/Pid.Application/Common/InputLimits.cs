namespace Pid.Application.Common;

/// <summary>
/// Sensible ranges for user input. These are application rules (what the UI accepts),
/// not physical laws, so they live here and not in the domain.
/// </summary>
public static class InputLimits
{
    public const double MinSetpointC = 5;
    public const double MaxSetpointC = 35;

    public const double MinAmbientC = -40;
    public const double MaxAmbientC = 45;

    public const double MinInitialTemperatureC = -40;
    public const double MaxInitialTemperatureC = 60;

    public static readonly TimeSpan MaxAdvanceDuration = TimeSpan.FromDays(7);
}
