namespace ReconfigurableMiddleware;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Utils;

public static class StartupExtensions
{
    public static IApplicationBuilder UseReloadablePipeline<TOptions>(this IApplicationBuilder builder,
        Action<IApplicationBuilder, IWebHostEnvironment, TOptions> configure)
        where TOptions : class
    {
        return AddReloadablePipeline<TOptions>(builder, configure, false);
    }

    public static IApplicationBuilder RunReloadablePipeline<TOptions>(this IApplicationBuilder builder, Action<IApplicationBuilder, IWebHostEnvironment, TOptions> configure)
        where TOptions : class
    {
        return AddReloadablePipeline<TOptions>(builder, configure, true);
    }

    /// <summary>
    /// Adds <see cref="ReloadPipelineMiddleware"/> to the middleware pipeline, with a change token to invalidate it and rebuild it whenever <typeparamref name="TOptions"/> changes.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    /// <param name="builder"></param>
    /// <param name="configure"></param>
    /// <param name="isTerminal"></param>
    /// <returns></returns>
    /// <remarks>You must ensure <typeparamref name="TOptions"/></remarks> has been registered with the options system in ConfigureServices.
    private static IApplicationBuilder AddReloadablePipeline<TOptions>(this IApplicationBuilder builder, Action<IApplicationBuilder, IWebHostEnvironment, TOptions> configure, bool isTerminal)
        where TOptions : class
    {
        var env = builder.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
        var monitor = builder.ApplicationServices.GetRequiredService<IOptionsMonitor<TOptions>>();

        IDisposable? previousRegistration = null;

        var factory = new RequestDelegateFactory(env, () =>
        {
            // When should ensure any previous CancellationTokenSource is disposed,
            // and we remove old monitor OnChange listener, before creating new ones.
            previousRegistration?.Dispose();

            var changeTokenSource = new CancellationTokenSource();
            var monitorListener = monitor.OnChange(a => changeTokenSource.Cancel());

            previousRegistration = new InvokeOnDispose(() =>
            {
                // Ensure disposal of listener and token source that we created.
                monitorListener?.Dispose();
                changeTokenSource.Dispose();
            });

            var changeToken = new CancellationChangeToken(changeTokenSource.Token);
            return changeToken;

        }, (a, b) =>
        {
            configure(a, b, monitor.CurrentValue);
        });

        builder.UseMiddleware<ReloadPipelineMiddleware>(builder, factory, isTerminal);
        return builder;
    }
}
