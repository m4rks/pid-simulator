using Pid.Domain.Common;

namespace Pid.Domain.Controllers;

public sealed class PidController(PidParameters parameters) : IController
{
    private const double MinOutput = 0.0;
    private const double MaxOutput = 1.0;

    // Stores the sum of Ki * error * dt, so changing Ki later does not make the output jump.
    private double _integral;
    private double? _previousMeasuredC;

    public PidParameters Parameters { get; private set; } = parameters;

    public double Compute(double setpointC, double measuredC, TimeSpan dt)
    {
        var dtSeconds = TimeStep.ToPositiveSeconds(dt);
        var error = setpointC - measuredC;

        var proportional = Parameters.Kp * error;

        // Derivative on measurement (not on error): changing the setpoint does not cause an output spike.
        var derivative = _previousMeasuredC is { } previous
            ? -Parameters.Kd * (measuredC - previous) / dtSeconds
            : 0.0;
        _previousMeasuredC = measuredC;

        // Anti-windup: while the output is saturated, stop integrating in the direction that would
        // push it even further. Otherwise the integral grows huge and causes a large overshoot later.
        var candidateIntegral = _integral + Parameters.Ki * error * dtSeconds;
        var unclamped = proportional + candidateIntegral + derivative;
        var windsUp = (unclamped > MaxOutput && error > 0) || (unclamped < MinOutput && error < 0);
        if (!windsUp)
            _integral = candidateIntegral;

        return Math.Clamp(proportional + _integral + derivative, MinOutput, MaxOutput);
    }

    /// <summary>Retunes the controller without resetting its state, so the output does not jump.</summary>
    public void UpdateParameters(PidParameters parameters) => Parameters = parameters;

    public void Reset()
    {
        _integral = 0.0;
        _previousMeasuredC = null;
    }
}
