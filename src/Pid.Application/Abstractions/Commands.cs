namespace Pid.Application.Abstractions;

/// <summary>Marker for a request that changes state and returns no data.</summary>
public interface ICommand;

public interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
