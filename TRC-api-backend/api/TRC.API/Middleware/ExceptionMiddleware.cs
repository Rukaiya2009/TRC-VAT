using System.Text.Json;
using TRC.Shared.Common;

namespace TRC.API.Middleware;

// Global handler: sanitized error + correlation id (NFR-2).
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _log;

    // Match the camelCase the MVC pipeline uses, so error bodies and success bodies
    // have the same shape on the wire ({ success, data, errors, correlationId }).
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

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
        catch (InvalidOperationException ex)      // expected domain/validation errors
        {
            await Write(ctx, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (UnauthorizedAccessException ex)    // e.g. touching another phone's booking
        {
            await Write(ctx, StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (ArgumentException ex)              // e.g. unparseable phone number
        {
            await Write(ctx, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            var response = ApiResponse<object>.Fail("An unexpected error occurred.");
            _log.LogError(ex, "Unhandled exception. CorrelationId={CorrelationId}", response.CorrelationId);
            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(response, Json));
        }
    }

    private static async Task Write(HttpContext ctx, int status, string message)
    {
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(ApiResponse<object>.Fail(message), Json));
    }
}
