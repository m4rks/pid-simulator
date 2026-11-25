using FluentValidation;

namespace Pid.Application.Common;

internal static class RuleBuilderExtensions
{
    /// <summary>Rejects NaN and infinity, which would silently break the simulation math.</summary>
    public static IRuleBuilderOptions<T, double> Finite<T>(this IRuleBuilder<T, double> rule) =>
        rule.Must(double.IsFinite).WithMessage("'{PropertyName}' must be a finite number.");
}
