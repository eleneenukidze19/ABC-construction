using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace ABC_construction.Middleware;

/// <summary>
/// Single catch-all for unhandled exceptions (README sections 19 and 22).
/// Logs the failure with a correlation id, then returns RFC 7807
/// <c>ProblemDetails</c> to API callers or redirects browsers to the error
/// page. Exception details are never sent to the client outside Development.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Client went away mid-request; this is normal, not an error.
            _logger.LogDebug("Request {Path} was cancelled by the client.", context.Request.Path);
        }
        catch (Exception ex)
        {
            var correlationId = Activity.Current?.Id ?? context.TraceIdentifier;

            _logger.LogError(
                ex,
                "Unhandled exception for {Method} {Path} (correlation {CorrelationId}).",
                context.Request.Method,
                context.Request.Path,
                correlationId);

            if (context.Response.HasStarted)
            {
                // Too late to rewrite the response; let the server tear it down.
                throw;
            }

            await WriteErrorResponseAsync(context, correlationId, ex);
        }
    }

    private async Task WriteErrorResponseAsync(HttpContext context, string correlationId, Exception ex)
    {
        context.Response.Clear();
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        if (IsApiRequest(context))
        {
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Instance = context.Request.Path,
                // Only Development sees the exception text.
                Detail = _environment.IsDevelopment() ? ex.ToString() : null
            };

            problem.Extensions["correlationId"] = correlationId;

            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
            return;
        }

        context.Response.Redirect($"/Home/Error?correlationId={Uri.EscapeDataString(correlationId)}");
    }

    /// <summary>
    /// Treats /api/* and requests that ask for JSON as API calls, so a fetch()
    /// from the admin panel gets JSON rather than an HTML redirect.
    /// </summary>
    private static bool IsApiRequest(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var accept = context.Request.Headers.Accept.ToString();
        return accept.Contains("application/json", StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>Registration helper so Program.cs reads as a pipeline description.</summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionHandlingMiddleware>();
}
