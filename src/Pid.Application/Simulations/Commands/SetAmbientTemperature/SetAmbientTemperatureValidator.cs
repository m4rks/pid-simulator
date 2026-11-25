using FluentValidation;
using Pid.Application.Common;

namespace Pid.Application.Simulations.Commands.SetAmbientTemperature;

public sealed class SetAmbientTemperatureValidator : AbstractValidator<SetAmbientTemperatureCommand>
{
    public SetAmbientTemperatureValidator()
    {
        RuleFor(x => x.SimulationId).NotEmpty();
        RuleFor(x => x.AmbientC).InclusiveBetween(InputLimits.MinAmbientC, InputLimits.MaxAmbientC);
    }
}
