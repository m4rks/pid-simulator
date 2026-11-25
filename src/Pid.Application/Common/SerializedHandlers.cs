using Pid.Application.Abstractions;

namespace Pid.Application.Common;

/// <summary>Decorator: runs the wrapped command handler inside <see cref="ExecutionGate"/>.</summary>
internal sealed class SerializedCommandHandler<TCommand>(ICommandHandler<TCommand> inner, ExecutionGate gate)
    : ICommandHandler<TCommand>
    where TCommand : ICommand
{
    public Task HandleAsync(TCommand command, CancellationToken cancellationToken = default) =>
        gate.RunAsync(() => inner.HandleAsync(command, cancellationToken), cancellationToken);
}

/// <summary>Decorator: runs the wrapped query handler inside <see cref="ExecutionGate"/>.</summary>
internal sealed class SerializedQueryHandler<TQuery, TResult>(IQueryHandler<TQuery, TResult> inner, ExecutionGate gate)
    : IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    public Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default) =>
        gate.RunAsync(() => inner.HandleAsync(query, cancellationToken), cancellationToken);
}
