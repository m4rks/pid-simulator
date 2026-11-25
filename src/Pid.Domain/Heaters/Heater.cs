using Pid.Domain.Common;

namespace Pid.Domain.Heaters;

/// <summary>Furnace: turns a control signal 0..1 into heat power.</summary>
public sealed class Heater
{
    public double MaxPowerW { get; }

    public Heater(double maxPowerW)
    {
        if (maxPowerW <= 0)
            throw new DomainException("Heater power must be positive.");

        MaxPowerW = maxPowerW;
    }

    public double GetPowerW(double controlSignal) => Math.Clamp(controlSignal, 0.0, 1.0) * MaxPowerW;
}
