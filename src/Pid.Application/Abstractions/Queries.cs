namespace Pid.Application.Abstractions;

/// <summary>Marker for a read-only request returning <typeparamref name="TResult"/>.</summary>
public interface IQuery<TResult>;

public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
