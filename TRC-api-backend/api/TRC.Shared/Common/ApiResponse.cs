namespace TRC.Shared.Common;

// Standard response envelope for every endpoint (SRS §11).
public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public IEnumerable<string> Errors { get; init; } = Array.Empty<string>();
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString("N");

    public static ApiResponse<T> Ok(T data) => new() { Success = true, Data = data };
    public static ApiResponse<T> Fail(params string[] errors) => new() { Success = false, Errors = errors };
}
