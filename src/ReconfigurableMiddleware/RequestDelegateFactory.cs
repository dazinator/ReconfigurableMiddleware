using Microsoft.Extensions.Primitives;
using ReconfigurableMiddleware.Utils;

namespace ReconfigurableMiddleware;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

public class RequestDelegateFactory : IDisposable
{
    private readonly IWebHostEnvironment _environment;
    private readonly Action<IApplicationBuilder, IWebHostEnvironment> _configure;
    private readonly IDisposable _listening = null;
    private RequestDelegate _currentRequestDelegate = null;
    private readonly object _currentInstanceLock = new object();

    public IApplicationBuilder _subBuilder { get; set; }

    public RequestDelegateFactory(IWebHostEnvironment environment,
        Func<IChangeToken> getNewChangeToken,
        Action<IApplicationBuilder, IWebHostEnvironment> configure)
    {
        _environment = environment;
        _configure = configure;
        _listening = ChangeTokenHelper.OnChangeDebounce(getNewChangeToken, InvokeChanged, delayInMilliseconds: 500);
    }

    private void InvokeChanged()
    {
        // Option - rebuild in background, or wait until first request?
        // waiting until first request for now.
        _currentRequestDelegate = null;
    }

    public RequestDelegate Get(IApplicationBuilder builder, RequestDelegate onNext, bool isTerminal)
    {
        var existing = _currentRequestDelegate;
        if (existing != null)
        {
            return existing;
        }

        // Only allow one build at a time.
        lock (_currentInstanceLock)
        {
            if (existing != null)
            {
                return existing;
            }

            _subBuilder = builder.New();

            _configure(_subBuilder, _environment);

            // if nothing in this pipeline runs, join back to root pipeline?
            if (!isTerminal && onNext != null)
            {
                _subBuilder.Run(onNext);
                //_subBuilder.Run(async (http) => await onNext());
            }

            var newInstance = _subBuilder.Build();
            _currentRequestDelegate = newInstance;


            // as we don't lock in Invalidate(), it could have just set _currentRequestDelegate back to null here,
            // that's why we keep hold of and return, newInstance - as this method must always return an instance to satisfy current request.
            return newInstance;
        }
    }

    public void Dispose() => _listening?.Dispose();
}
