using Pid.Application.Simulations.Dtos;

namespace Pid.Api.Endpoints;

// HTTP request/response bodies. The simulation id comes from the URL, so it is not repeated here.

public sealed record CreateSimulationRequest(
    HouseParametersDto House,
    double InitialTemperatureC,
    double HeaterMaxPowerW,
    PidParametersDto Pid,
    double SetpointC,
    double AmbientC);

public sealed record CreateSimulationResponse(Guid Id);

public sealed record SetSetpointRequest(double SetpointC);

public sealed record SetAmbientTemperatureRequest(double AmbientC);

/// <param name="Duration">JSON format "hh:mm:ss", e.g. "01:00:00".</param>
public sealed record AdvanceSimulationRequest(TimeSpan Duration);
