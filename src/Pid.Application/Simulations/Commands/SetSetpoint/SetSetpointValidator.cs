using FluentValidation;
using Pid.Application.Common;

namespace Pid.Application.Simulations.Commands.SetSetpoint;

public sealed class SetSetpointValidator : AbstractValidator<SetSetpointCommand>
{
    public SetSetpointValidator()
    {
        RuleFor(x => x.SimulationId).NotEmpty();
        RuleFor(x => x.SetpointC).InclusiveBetween(InputLimits.MinSetpointC, InputLimits.MaxSetpointC);
    }
}
