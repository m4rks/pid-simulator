using FluentValidation;
using Pid.Application.Simulations.Dtos;

namespace Pid.Application.Simulations.Commands.UpdatePidParameters;

public sealed class UpdatePidParametersValidator : AbstractValidator<UpdatePidParametersCommand>
{
    public UpdatePidParametersValidator()
    {
        RuleFor(x => x.SimulationId).NotEmpty();
        RuleFor(x => x.Pid).NotNull().SetValidator(new PidParametersDtoValidator());
    }
}
