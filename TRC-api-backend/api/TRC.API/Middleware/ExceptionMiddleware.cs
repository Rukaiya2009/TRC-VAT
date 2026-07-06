using System.Text.Json;
using TRC.Shared.Common;

namespace TRC.API.Middleware;

// Global handler: sanitized error + correlation id (NFR-2).
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _log;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> log)
    {
        _next = next;
        _log = log;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (InvalidOperationException ex)   // expected domain/validation errors
        {
            await Write(ctx, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            var response = ApiResponse<object>.Fail("An unexpected error occurred.");
            _log.LogError(ex, "Unhandled exception. CorrelationId={CorrelationId}", response.CorrelationId);
            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }

    private static async Task Write(HttpContext ctx, int status, string message)
    {
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(ApiResponse<object>.Fail(message)));
    }
}
