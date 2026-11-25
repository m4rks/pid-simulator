using System.Net.Http.Json;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Pid.Api.Endpoints;
using Pid.Application.Simulations.Dtos;

namespace Pid.Api.Tests;

/// <summary>End to end: runner -> Application -> SignalR -> client.</summary>
public class SimulationHubTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Subscribed_client_receives_state_updates_from_the_runner()
    {
        var client = factory.CreateClient();
        var created = await (await client.PostAsJsonAsync("/api/simulations", SimulationApiTests.ValidRequest))
            .Content.ReadFromJsonAsync<CreateSimulationResponse>();

        await using var connection = new HubConnectionBuilder()
            .WithUrl(new Uri(factory.Server.BaseAddress, "hubs/simulation"), options =>
            {
                // The in-memory test server has no real network, so route SignalR through its handler.
                options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
                options.Transports = HttpTransportType.LongPolling;
            })
            .Build();

        var received = new TaskCompletionSource<SimulationStateDto>(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.On<SimulationStateDto>("StateUpdated", state => received.TrySetResult(state));
        await connection.StartAsync();
        await connection.InvokeAsync("Subscribe", created!.Id);

        var update = await received.Task.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.Equal(created.Id, update.Id);
        Assert.True(update.TimeSeconds > 0);
    }
}
