using Pid.Domain.Common;

namespace Pid.Domain.Plants;

/// <summary>
/// First-order thermal model of a house: C * dT/dt = P - (T - T_amb) / R.
/// </summary>
public sealed class HouseModel(HouseParameters parameters, double initialTemperatureC) : IThermalPlant
{
    public HouseParameters Parameters { get; } = parameters;
    public double TemperatureC { get; private set; } = initialTemperatureC;

    public double Step(double heatPowerW, double ambientC, TimeSpan dt)
    {
        var dtSeconds = TimeStep.ToPositiveSeconds(dt);
        var r = Parameters.ThermalResistanceKPerW;

        // Exact solution for inputs held constant during dt (instead of Euler's T += dT/dt * dt).
        // It stays accurate and stable for any dt, even one longer than the time constant.
        var steadyStateC = ambientC + heatPowerW * r;
        var decay = Math.Exp(-dtSeconds / Parameters.TimeConstant.TotalSeconds);
        TemperatureC = steadyStateC + (TemperatureC - steadyStateC) * decay;

        return TemperatureC;
    }
}
