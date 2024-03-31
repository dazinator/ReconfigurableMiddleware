namespace ReconfigurableMiddleware.Utils;

using Microsoft.Extensions.Primitives;

public static class ChangeTokenHelper
{
    private const int DefaultDelayInMilliseconds = 500;

    /// <summary>
    /// Handle <see cref="ChangeToken.OnChange{TState}(Func{IChangeToken}, Action{TState}, TState)"/> after a delay that discards any duplicate invocations within that period of time.
    /// Useful for working around issue like described here: https://github.com/aspnet/AspNetCore/issues/2542
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="changeTokenFactory"></param>
    /// <param name="listener"></param>
    /// <param name="state"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    public static IDisposable OnChangeDebounce<T>(Func<IChangeToken> changeTokenFactory, Action<T> listener, T state,
        int delayInMilliseconds = DefaultDelayInMilliseconds)
    {
        var debouncer = new Debouncer<T>(TimeSpan.FromMilliseconds(delayInMilliseconds));
        var token = ChangeToken.OnChange<T>(changeTokenFactory, s => debouncer.Debounce(listener, s), state);
        return token;
    }

    /// <summary>
    /// Handle <see cref="ChangeToken.OnChange(Func{IChangeToken}, Action)"/> after a delay that discards any duplicate invocations within that period of time.
    /// Useful for working around issue like described here: https://github.com/aspnet/AspNetCore/issues/2542
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="changeTokenFactory"></param>
    /// <param name="listener"></param>
    /// <param name="state"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    public static IDisposable OnChangeDebounce(Func<IChangeToken> changeTokenFactory, Action listener,
        int delayInMilliseconds = DefaultDelayInMilliseconds)
    {
        var debouncer = new Debouncer(TimeSpan.FromMilliseconds(delayInMilliseconds));
        var token = ChangeToken.OnChange(changeTokenFactory, () => debouncer.Debouce(listener));
        return token;
    }
}

/// <summary>
/// Courtesy of @cocowalla https://gist.github.com/cocowalla/5d181b82b9a986c6761585000901d1b8
/// </summary>
public class Debouncer : IDisposable
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly TimeSpan waitTime;
    private int counter;

    public Debouncer(TimeSpan? waitTime = null)
    {
        this.waitTime = waitTime ?? TimeSpan.FromSeconds(1);
    }

    public void Debouce(Action action)
    {
        int current = Interlocked.Increment(ref counter);

        Task.Delay(waitTime).ContinueWith(task =>
        {
            // Is this the last task that was queued?
            if (current == counter && !cts.IsCancellationRequested)
            {
                action();
            }
            else
            {
                // Console.WriteLine("debounced..");
            }
#if NETSTANDARD2_0
                  task.Dispose();
#endif
        }, cts.Token);
    }

    public void Dispose()
    {
        cts.Cancel();
    }
}
