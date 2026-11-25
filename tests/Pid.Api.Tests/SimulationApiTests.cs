using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Pid.Api.Endpoints;
using Pid.Application.Simulations.Dtos;

namespace Pid.Api.Tests;

/// <summary>Starts the real application in memory and talks to it over HTTP.</summary>
public class SimulationApiTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    public static CreateSimulationRequest ValidRequest => new(
        House: new HouseParametersDto(HeatCapacityJPerK: 2e7, ThermalResistanceKPerW: 0.005),
        InitialTemperatureC: 15,
        HeaterMaxPowerW: 10000,
        Pid: new PidParametersDto(Kp: 0.5, Ki: 0.5 / 3600, Kd: 0),
        SetpointC: 21,
        AmbientC: 0);

    [Fact]
    public async Task Create_returns_201_and_the_simulation_can_be_read()
    {
        var response = await _client.PostAsJsonAsync("/api/simulations", ValidRequest);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CreateSimulationResponse>();
        Assert.Equal($"/api/simulations/{created!.Id}", response.Headers.Location!.ToString());

        var state = await _client.GetFromJsonAsync<SimulationStateDto>(response.Headers.Location);
        Assert.Equal(created.Id, state!.Id);
        Assert.Equal(21, state.SetpointC);
        Assert.Equal(ValidRequest.Pid, state.Pid);
    }

    [Fact]
    public async Task Created_simulation_is_listed()
    {
        var id = await CreateSimulation();

        var ids = await _client.GetFromJsonAsync<Guid[]>("/api/simulations");

        Assert.Contains(id, ids!);
    }

    [Fact]
    public async Task Invalid_create_returns_400_with_field_errors()
    {
        var response = await _client.PostAsJsonAsync("/api/simulations", ValidRequest with { SetpointC = 500 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        Assert.Contains("SetpointC", problem!.Errors.Keys);
    }

    [Fact]
    public async Task Malformed_json_returns_400()
    {
        var response = await _client.PostAsync("/api/simulations",
            new StringContent("{ not json", System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Unknown_simulation_returns_404()
    {
        var response = await _client.GetAsync($"/api/simulations/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Setpoint_ambient_and_pid_can_be_changed()
    {
        var id = await CreateSimulation();
        var newPid = new PidParametersDto(Kp: 1, Ki: 0.0003, Kd: 5);

        var responses = new[]
        {
            await _client.PutAsJsonAsync($"/api/simulations/{id}/setpoint", new SetSetpointRequest(18)),
            await _client.PutAsJsonAsync($"/api/simulations/{id}/ambient", new SetAmbientTemperatureRequest(-15)),
            await _client.PutAsJsonAsync($"/api/simulations/{id}/pid", newPid),
        };

        Assert.All(responses, r => Assert.Equal(HttpStatusCode.NoContent, r.StatusCode));
        var state = await _client.GetFromJsonAsync<SimulationStateDto>($"/api/simulations/{id}");
        Assert.Equal(18, state!.SetpointC);
        Assert.Equal(-15, state.AmbientC);
        Assert.Equal(newPid, state.Pid);
    }

    [Fact]
    public async Task Advance_moves_simulated_time_and_fills_history()
    {
        var id = await CreateSimulation();

        var response = await _client.PostAsJsonAsync(
            $"/api/simulations/{id}/advance", new AdvanceSimulationRequest(TimeSpan.FromHours(1)));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        // The background runner may have advanced it a bit more in the meantime.
        var state = await _client.GetFromJsonAsync<SimulationStateDto>($"/api/simulations/{id}");
        Assert.True(state!.TimeSeconds >= 3600);
        var history = await _client.GetFromJsonAsync<SimulationSampleDto[]>($"/api/simulations/{id}/history?maxSamples=5");
        Assert.Equal(5, history!.Length);
    }

    [Fact]
    public async Task Parallel_commands_and_queries_do_not_corrupt_the_simulation()
    {
        var id = await CreateSimulation();

        // Without ExecutionGate, reading history while another request appends to it would fail randomly.
        var requests = Enumerable.Range(0, 20).SelectMany(_ => new[]
        {
            _client.PostAsJsonAsync($"/api/simulations/{id}/advance", new AdvanceSimulationRequest(TimeSpan.FromHours(2))),
            _client.GetAsync($"/api/simulations/{id}/history?maxSamples=20000"),
        });
        var responses = await Task.WhenAll(requests);

        Assert.All(responses, r => Assert.True(r.IsSuccessStatusCode, $"{r.RequestMessage!.RequestUri}: {r.StatusCode}"));
    }

    private async Task<Guid> CreateSimulation()
    {
        var response = await _client.PostAsJsonAsync("/api/simulations", ValidRequest);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CreateSimulationResponse>())!.Id;
    }
}
