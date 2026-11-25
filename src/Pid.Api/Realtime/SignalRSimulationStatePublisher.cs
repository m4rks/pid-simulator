using Microsoft.AspNetCore.SignalR;
using Pid.Application.Abstractions;
using Pid.Application.Simulations.Dtos;

namespace Pid.Api.Realtime;

/// <summary>Implements the Application port with SignalR: sends the state to clients subscribed to that simulation.</summary>
internal sealed class SignalRSimulationStatePublisher(IHubContext<SimulationHub, ISimulationClient> hub)
    : ISimulationStatePublisher
{
    public Task PublishAsync(SimulationStateDto state, CancellationToken cancellationToken = default) =>
        hub.Clients.Group(SimulationHub.GroupName(state.Id)).StateUpdated(state);
}
