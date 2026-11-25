using Pid.Domain.Common;

namespace Pid.Domain.Controllers;

/// <param name="Kp">Proportional gain [1/K]: output per 1 K of error.</param>
/// <param name="Ki">Integral gain [1/(K*s)]: how fast the output grows while the error persists.</param>
/// <param name="Kd">Derivative gain [s/K]: reaction to how fast the temperature changes.</param>
public sealed record PidParameters
{
    public double Kp { get; }
    public double Ki { get; }
    public double Kd { get; }

    public PidParameters(double kp, double ki, double kd)
    {
        if (kp < 0 || ki < 0 || kd < 0)
            throw new DomainException("PID gains cannot be negative.");

        Kp = kp;
        Ki = ki;
        Kd = kd;
    }
}
