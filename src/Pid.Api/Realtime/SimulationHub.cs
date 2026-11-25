using Microsoft.AspNetCore.SignalR;
using Pid.Application.Simulations.Dtos;

namespace Pid.Api.Realtime;

/// <summary>Methods the server can call on the browser. Typed, so a typo is a compile error.</summary>
public interface ISimulationClient
{
    Task StateUpdated(SimulationStateDto state);
}

/// <summary>
/// The browser connects to /hubs/simulation and subscribes to one simulation;
/// from then on it receives StateUpdated on every runner tick.
/// </summary>
public sealed class SimulationHub : Hub<ISimulationClient>
{
    public Task Subscribe(Guid simulationId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, GroupName(simulationId));

    public Task Unsubscribe(Guid simulationId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(simulationId));

    internal static string GroupName(Guid simulationId) => $"simulation-{simulationId}";
}
