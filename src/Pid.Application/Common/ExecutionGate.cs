namespace Pid.Application.Common;

/// <summary>
/// Lets only one handler run at a time. Simulations are plain in-memory objects shared by
/// API requests and the background runner; without this, two threads could change the same
/// simulation at once (e.g. a query reading history while a tick appends to it).
/// With a database this role would be played by transactions.
/// </summary>
internal sealed class ExecutionGate
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task<T> RunAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await action();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public Task RunAsync(Func<Task> action, CancellationToken cancellationToken) =>
        RunAsync(async () =>
        {
            await action();
            return true;
        }, cancellationToken);
}
