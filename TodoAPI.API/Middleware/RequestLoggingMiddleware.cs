namespace TodoAPI.API.Middleware;

public class RequestLoggingMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Items[CorrelationIdHeader]?.ToString() ?? "unknown";
        var method = context.Request.Method;
        var path = context.Request.Path;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        _logger.LogInformation(
            "[{CorrelationId}] IN {Method} {Path}",
            correlationId, method, path);

        try
        {
            await _next(context);
            stopwatch.Stop();

            var level = context.Response.StatusCode >= 500
                ? LogLevel.Error
                : context.Response.StatusCode >= 400
                    ? LogLevel.Warning
                    : LogLevel.Information;

            _logger.Log(level,
                "[{CorrelationId}] OUT {Method} {Path} {StatusCode} ({Duration}ms)",
                correlationId, method, path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex,
                "[{CorrelationId}] ERROR {Method} {Path} EXCEPTION ({Duration}ms)",
                correlationId, method, path,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}