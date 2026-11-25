using FluentValidation;
using Pid.Application.Common;
using Pid.Application.Simulations.Dtos;

namespace Pid.Application.Simulations.Commands.CreateSimulation;

public sealed class CreateSimulationValidator : AbstractValidator<CreateSimulationCommand>
{
    public CreateSimulationValidator()
    {
        RuleFor(x => x.SimulationId).NotEmpty();
        RuleFor(x => x.House).NotNull().SetValidator(new HouseParametersDtoValidator());
        RuleFor(x => x.InitialTemperatureC)
            .InclusiveBetween(InputLimits.MinInitialTemperatureC, InputLimits.MaxInitialTemperatureC);
        RuleFor(x => x.HeaterMaxPowerW).Finite().GreaterThan(0);
        RuleFor(x => x.Pid).NotNull().SetValidator(new PidParametersDtoValidator());
        RuleFor(x => x.SetpointC).InclusiveBetween(InputLimits.MinSetpointC, InputLimits.MaxSetpointC);
        RuleFor(x => x.AmbientC).InclusiveBetween(InputLimits.MinAmbientC, InputLimits.MaxAmbientC);
    }
}
