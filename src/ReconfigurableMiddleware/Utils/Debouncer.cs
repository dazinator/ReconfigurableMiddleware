namespace ReconfigurableMiddleware.Utils;

public class Debouncer<T> : IDisposable
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly TimeSpan waitTime;
    private int counter;

    public Debouncer(TimeSpan? waitTime = null)
    {
        this.waitTime = waitTime ?? TimeSpan.FromSeconds(1);
    }

    public void Debounce(Action<T> action, T state)
    {
        int current = Interlocked.Increment(ref counter);

        if(counter > 1000)
        {
            // detect error
            Console.WriteLine("debounc limit exceeded: " + counter);
            return;
        }
        Task.Delay(waitTime).ContinueWith(task =>
        {
            // Is this the last task that was queued?
            if (current == counter && !cts.IsCancellationRequested)
            {
                action(state);
            }               
            else
            {
                Console.WriteLine("debounced..");
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
