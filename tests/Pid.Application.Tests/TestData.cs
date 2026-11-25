using Pid.Application.Simulations.Commands.CreateSimulation;
using Pid.Application.Simulations.Dtos;

namespace Pid.Application.Tests;

internal static class TestData
{
    public static CreateSimulationCommand CreateCommand(Guid? id = null) => new(
        SimulationId: id ?? Guid.NewGuid(),
        House: new HouseParametersDto(HeatCapacityJPerK: 2e7, ThermalResistanceKPerW: 0.005),
        InitialTemperatureC: 15,
        HeaterMaxPowerW: 10000,
        Pid: new PidParametersDto(Kp: 0.5, Ki: 0.5 / 3600, Kd: 0),
        SetpointC: 21,
        AmbientC: 0);
}
