using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace BuildingBlocks.Models;

/// <summary>
/// Standard API Response wrapper.
/// </summary>
public class ApiResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("statusCode")]
    [JsonProperty("statusCode")]
    public int StatusCode { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("message")]
    [JsonProperty("message")]
    public string Message { get; set; }
    
    /// <summary>
    /// Specific business error code (e.g., ORD_NOT_FOUND).
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("errorCode")]
    [JsonProperty("errorCode", NullValueHandling = NullValueHandling.Ignore)]
    [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ErrorCode { get; set; }

    [System.Text.Json.Serialization.JsonInclude]
    [System.Text.Json.Serialization.JsonPropertyName("success")]
    [JsonProperty("success")]
    public bool Success { get; private set; }

    [JsonProperty("errors", NullValueHandling = NullValueHandling.Ignore)]
    [System.Text.Json.Serialization.JsonPropertyName("errors")]
    [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Errors { get; set; }

    public ApiResponse(int statusCode, string message, object? errors = null, string? errorCode = null)
    {
        StatusCode = statusCode;
        Message = message;
        Errors = errors;
        ErrorCode = errorCode;
        Success = statusCode >= 200 && statusCode < 300;
    }
}

/// <summary>
/// Generic wrapper for API responses with data payload.
/// </summary>
public class ApiResponse<T> : ApiResponse
{
    [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
    [System.Text.Json.Serialization.JsonPropertyName("data")]
    [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; }

    public ApiResponse(int statusCode, string message, T? data = default, object? errors = null, string? errorCode = null) 
        : base(statusCode, message, errors, errorCode)
    {
        Data = data;
    }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Success", int statusCode = 200) 
        => new(statusCode, message, data);

    public static ApiResponse<T> CreatedResponse(T data, string message = "Created") 
        => new(201, message, data);

    public static ApiResponse<T> FailureResponse(int statusCode, string message, string? errorCode = null, object? errors = null) 
        => new(statusCode, message, default!, errors, errorCode);

    public static ApiResponse<T> NotFoundResponse(string message = "Resource not found", string? errorCode = null)
        => new(404, message, default!, null, errorCode);

    public static ApiResponse<T> InternalErrorResponse(string message = "An internal server error occurred", string? errorCode = "SYS_INTERNAL_ERROR", object? errors = null)
        => new(500, message, default!, errors, errorCode);
}
