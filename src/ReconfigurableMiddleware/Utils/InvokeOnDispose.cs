namespace ReconfigurableMiddleware.Utils;

internal class InvokeOnDispose : IDisposable
{
    private readonly Action _onDispose;

    public InvokeOnDispose(Action onDispose)
    {
        _onDispose = onDispose;
    }

    public void Dispose()
    {
        _onDispose?.Invoke();
    }
}
