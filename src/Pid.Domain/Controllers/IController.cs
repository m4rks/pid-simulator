namespace Pid.Domain.Controllers;

public interface IController
{
    /// <summary>Returns the control signal (0..1) for the current measurement.</summary>
    double Compute(double setpointC, double measuredC, TimeSpan dt);

    /// <summary>Clears internal state (integral, previous measurement).</summary>
    void Reset();
}
