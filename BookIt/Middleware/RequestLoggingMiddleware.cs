using System.Diagnostics;

namespace BookIt.Middleware;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = Guid.NewGuid().ToString();
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            return Task.CompletedTask;
        });

        var stopwatch = Stopwatch.StartNew();
        using (logger.BeginScope("CorrelationId:{CorrelationId}", correlationId))
        {
            try
            {
                await next(context);
            }
            finally
            {
                stopwatch.Stop();
                logger.LogInformation(
                    "{Method} {Path} responded {StatusCode} in {ElapsedMilliseconds}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
