using Pid.Api.Endpoints;
using Pid.Api.ErrorHandling;
using Pid.Api.Realtime;
using Pid.Application;
using Pid.Application.Abstractions;
using Pid.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddSignalR();
builder.Services.AddSingleton<ISimulationStatePublisher, SignalRSimulationStatePublisher>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();
app.MapSimulationEndpoints();
app.MapHub<SimulationHub>("/hubs/simulation");

app.Run();

// Lets the integration tests start the app with WebApplicationFactory<Program>.
public partial class Program;
