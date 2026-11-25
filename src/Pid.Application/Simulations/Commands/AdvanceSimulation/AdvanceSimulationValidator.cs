using FluentValidation;
using Pid.Application.Common;

namespace Pid.Application.Simulations.Commands.AdvanceSimulation;

public sealed class AdvanceSimulationValidator : AbstractValidator<AdvanceSimulationCommand>
{
    public AdvanceSimulationValidator()
    {
        RuleFor(x => x.SimulationId).NotEmpty();
        RuleFor(x => x.Duration).GreaterThan(TimeSpan.Zero).LessThanOrEqualTo(InputLimits.MaxAdvanceDuration);
    }
}
