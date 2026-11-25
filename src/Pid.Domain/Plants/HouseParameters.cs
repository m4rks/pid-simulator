using Pid.Domain.Common;

namespace Pid.Domain.Plants;

/// <param name="HeatCapacityJPerK">Energy needed to warm the whole house by 1 K.</param>
/// <param name="ThermalResistanceKPerW">How well the house is insulated; heat loss = (T - T_amb) / R.</param>
public sealed record HouseParameters
{
    public double HeatCapacityJPerK { get; }
    public double ThermalResistanceKPerW { get; }

    public HouseParameters(double heatCapacityJPerK, double thermalResistanceKPerW)
    {
        if (heatCapacityJPerK <= 0)
            throw new DomainException("Heat capacity must be positive.");
        if (thermalResistanceKPerW <= 0)
            throw new DomainException("Thermal resistance must be positive.");

        HeatCapacityJPerK = heatCapacityJPerK;
        ThermalResistanceKPerW = thermalResistanceKPerW;
    }

    /// <summary>Time constant R*C: after this time ~63% of a temperature change has happened.</summary>
    public TimeSpan TimeConstant => TimeSpan.FromSeconds(HeatCapacityJPerK * ThermalResistanceKPerW);
}
