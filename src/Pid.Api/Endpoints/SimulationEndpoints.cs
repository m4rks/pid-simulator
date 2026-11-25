using Pid.Application.Abstractions;
using Pid.Application.Simulations.Commands.AdvanceSimulation;
using Pid.Application.Simulations.Commands.CreateSimulation;
using Pid.Application.Simulations.Commands.SetAmbientTemperature;
using Pid.Application.Simulations.Commands.SetSetpoint;
using Pid.Application.Simulations.Commands.UpdatePidParameters;
using Pid.Application.Simulations.Dtos;
using Pid.Application.Simulations.Queries.GetSimulationHistory;
using Pid.Application.Simulations.Queries.GetSimulationState;
using Pid.Application.Simulations.Queries.ListSimulations;

namespace Pid.Api.Endpoints;

/// <summary>
/// Thin endpoints: turn HTTP into a command or query and call its handler. No business logic here.
/// Errors (validation, not found) are turned into HTTP responses by ApiExceptionHandler.
/// </summary>
public static class SimulationEndpoints
{
    public static IEndpointRouteBuilder MapSimulationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/simulations");

        // Commands

        group.MapPost("/", async (
            CreateSimulationRequest request,
            ICommandHandler<CreateSimulationCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var id = Guid.NewGuid();
            await handler.HandleAsync(new CreateSimulationCommand(
                id, request.House, request.InitialTemperatureC, request.HeaterMaxPowerW,
                request.Pid, request.SetpointC, request.AmbientC), cancellationToken);

            return TypedResults.Created($"/api/simulations/{id}", new CreateSimulationResponse(id));
        });

        group.MapPut("/{id:guid}/setpoint", async (
            Guid id,
            SetSetpointRequest request,
            ICommandHandler<SetSetpointCommand> handler,
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(new SetSetpointCommand(id, request.SetpointC), cancellationToken);
            return TypedResults.NoContent();
        });

        group.MapPut("/{id:guid}/ambient", async (
            Guid id,
            SetAmbientTemperatureRequest request,
            ICommandHandler<SetAmbientTemperatureCommand> handler,
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(new SetAmbientTemperatureCommand(id, request.AmbientC), cancellationToken);
            return TypedResults.NoContent();
        });

        group.MapPut("/{id:guid}/pid", async (
            Guid id,
            PidParametersDto request,
            ICommandHandler<UpdatePidParametersCommand> handler,
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(new UpdatePidParametersCommand(id, request), cancellationToken);
            return TypedResults.NoContent();
        });

        group.MapPost("/{id:guid}/advance", async (
            Guid id,
            AdvanceSimulationRequest request,
            ICommandHandler<AdvanceSimulationCommand> handler,
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(new AdvanceSimulationCommand(id, request.Duration), cancellationToken);
            return TypedResults.NoContent();
        });

        // Queries

        group.MapGet("/", (
            IQueryHandler<ListSimulationsQuery, IReadOnlyList<Guid>> handler,
            CancellationToken cancellationToken) =>
            handler.HandleAsync(new ListSimulationsQuery(), cancellationToken));

        group.MapGet("/{id:guid}", (
            Guid id,
            IQueryHandler<GetSimulationStateQuery, SimulationStateDto> handler,
            CancellationToken cancellationToken) =>
            handler.HandleAsync(new GetSimulationStateQuery(id), cancellationToken));

        group.MapGet("/{id:guid}/history", (
            Guid id,
            int? maxSamples,
            IQueryHandler<GetSimulationHistoryQuery, IReadOnlyList<SimulationSampleDto>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = maxSamples is { } max
                ? new GetSimulationHistoryQuery(id, max)
                : new GetSimulationHistoryQuery(id);
            return handler.HandleAsync(query, cancellationToken);
        });

        return app;
    }
}
