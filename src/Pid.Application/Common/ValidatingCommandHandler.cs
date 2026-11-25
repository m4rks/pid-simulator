using FluentValidation;
using Pid.Application.Abstractions;

namespace Pid.Application.Common;

/// <summary>
/// Decorator: wraps a real handler and validates the command before it runs.
/// Handlers can then assume their input is valid.
/// </summary>
internal sealed class ValidatingCommandHandler<TCommand>(
    ICommandHandler<TCommand> inner,
    IEnumerable<IValidator<TCommand>> validators) : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    public async Task HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        foreach (var validator in validators)
            await validator.ValidateAndThrowAsync(command, cancellationToken);

        await inner.HandleAsync(command, cancellationToken);
    }
}
