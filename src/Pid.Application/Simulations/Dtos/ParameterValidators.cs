using FluentValidation;
using Pid.Application.Common;

namespace Pid.Application.Simulations.Dtos;

/// <summary>Shared by every command that carries PID parameters.</summary>
public sealed class PidParametersDtoValidator : AbstractValidator<PidParametersDto>
{
    public PidParametersDtoValidator()
    {
        RuleFor(x => x.Kp).Finite().GreaterThanOrEqualTo(0);
        RuleFor(x => x.Ki).Finite().GreaterThanOrEqualTo(0);
        RuleFor(x => x.Kd).Finite().GreaterThanOrEqualTo(0);
    }
}

public sealed class HouseParametersDtoValidator : AbstractValidator<HouseParametersDto>
{
    public HouseParametersDtoValidator()
    {
        RuleFor(x => x.HeatCapacityJPerK).Finite().GreaterThan(0);
        RuleFor(x => x.ThermalResistanceKPerW).Finite().GreaterThan(0);
    }
}
