using Pid.Domain.Controllers;
using Pid.Domain.Plants;
using Pid.Domain.Simulations;

namespace Pid.Application.Simulations.Dtos;

/// <summary>Translates between domain objects and DTOs, so entities never leave the Application layer.</summary>
internal static class SimulationMappings
{
    public static PidParameters ToDomain(this PidParametersDto dto) => new(dto.Kp, dto.Ki, dto.Kd);

    public static HouseParameters ToDomain(this HouseParametersDto dto) =>
        new(dto.HeatCapacityJPerK, dto.ThermalResistanceKPerW);

    public static PidParametersDto ToDto(this PidParameters parameters) =>
        new(parameters.Kp, parameters.Ki, parameters.Kd);

    public static HouseParametersDto ToDto(this HouseParameters parameters) =>
        new(parameters.HeatCapacityJPerK, parameters.ThermalResistanceKPerW);

    public static SimulationSampleDto ToDto(this SimulationSample sample) =>
        new(sample.Time.TotalSeconds, sample.TemperatureC, sample.SetpointC, sample.AmbientC,
            sample.ControlSignal, sample.HeaterPowerW);

    public static SimulationStateDto ToStateDto(this Simulation simulation)
    {
        var lastSample = simulation.History.LastOrDefault();

        return new SimulationStateDto(
            simulation.Id,
            simulation.Time.TotalSeconds,
            simulation.Plant.TemperatureC,
            simulation.SetpointC,
            simulation.AmbientC,
            lastSample?.ControlSignal ?? 0,
            lastSample?.HeaterPowerW ?? 0,
            simulation.Heater.MaxPowerW,
            (simulation.Plant as HouseModel)?.Parameters.ToDto(),
            (simulation.Controller as PidController)?.Parameters.ToDto());
    }
}
