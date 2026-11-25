namespace Pid.Domain.Plants;

/// <summary>A controlled object whose temperature changes with supplied heat and ambient temperature.</summary>
public interface IThermalPlant
{
    double TemperatureC { get; }

    /// <summary>Advances the model by <paramref name="dt"/> and returns the new temperature.</summary>
    double Step(double heatPowerW, double ambientC, TimeSpan dt);
}
