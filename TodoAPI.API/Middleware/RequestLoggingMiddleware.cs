using System.Diagnostics;

namespace TodoAPI.API.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("IN {Method} {Path}", method, path);

        try
        {
            await _next(context);
            stopwatch.Stop();

            var statusCode = context.Response.StatusCode;
            var elapsed = stopwatch.ElapsedMilliseconds;

            var level = statusCode >= 500
                ? LogLevel.Error
                : statusCode >= 400
                    ? LogLevel.Warning
                    : LogLevel.Information;

            _logger.Log(level,
                "OUT {Method} {Path} {StatusCode} ({Duration}ms)",
                method, path, statusCode, elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex,
                "ERROR {Method} {Path} EXCEPTION ({Duration}ms)",
                method, path, stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}