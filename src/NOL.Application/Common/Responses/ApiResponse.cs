using NOL.Domain.Enums;
using NOL.Application.Common.Interfaces;

namespace NOL.Application.Common.Responses;

public class ApiResponse<T>
{
    public bool Succeeded { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? InternalMessage { get; set; }
    public List<string> Errors { get; set; } = new();
    public string? StackTrace { get; set; }
    public T? Data { get; set; }
    public ApiStatusCode StatusCode { get; set; }
    public int StatusCodeValue => (int)StatusCode;

    public static ApiResponse<T> Success(T data, string? message = null, ApiStatusCode statusCode = ApiStatusCode.Success, ILocalizationService? localizationService = null)
    {
        var localizedMessage = message ?? localizationService?.GetLocalizedString("OperationSuccessful") ?? "Operation completed successfully";
        return new ApiResponse<T>
        {
            Succeeded = true,
            Message = localizedMessage,
            Data = data,
            StatusCode = statusCode
        };
    }

    public static ApiResponse<T> Error(string? message = null, List<string>? errors = null, ApiStatusCode statusCode = ApiStatusCode.BadRequest, ILocalizationService? localizationService = null)
    {
        var localizedMessage = message ?? localizationService?.GetLocalizedString("ValidationError") ?? "An error occurred";
        return new ApiResponse<T>
        {
            Succeeded = false,
            Message = localizedMessage,
            Errors = errors ?? new List<string>(),
            StatusCode = statusCode
        };
    }

    public static ApiResponse<T> Error(string? message = null, string? error = null, ApiStatusCode statusCode = ApiStatusCode.BadRequest, ILocalizationService? localizationService = null)
    {
        var localizedMessage = message ?? localizationService?.GetLocalizedString("ValidationError") ?? "An error occurred";
        return new ApiResponse<T>
        {
            Succeeded = false,
            Message = localizedMessage,
            Errors = error != null ? new List<string> { error } : new List<string>(),
            StatusCode = statusCode
        };
    }

    public static ApiResponse<T> NotFound(string? message = null, ILocalizationService? localizationService = null)
    {
        var localizedMessage = message ?? localizationService?.GetLocalizedString("ResourceNotFound") ?? "Resource not found";
        return new ApiResponse<T>
        {
            Succeeded = false,
            Message = localizedMessage,
            StatusCode = ApiStatusCode.NotFound
        };
    }

    public static ApiResponse<T> Unauthorized(string? message = null, ILocalizationService? localizationService = null)
    {
        var localizedMessage = message ?? localizationService?.GetLocalizedString("UnauthorizedAccess") ?? "Unauthorized access";
        return new ApiResponse<T>
        {
            Succeeded = false,
            Message = localizedMessage,
            StatusCode = ApiStatusCode.Unauthorized
        };
    }

    public static ApiResponse<T> Forbidden(string? message = null, ILocalizationService? localizationService = null)
    {
        var localizedMessage = message ?? localizationService?.GetLocalizedString("AccessForbidden") ?? "Access forbidden";
        return new ApiResponse<T>
        {
            Succeeded = false,
            Message = localizedMessage,
            StatusCode = ApiStatusCode.Forbidden
        };
    }

    public static ApiResponse<T> InternalServerError(string? message = null, ILocalizationService? localizationService = null)
    {
        var localizedMessage = message ?? localizationService?.GetLocalizedString("InternalServerError") ?? "Internal server error";
        return new ApiResponse<T>
        {
            Succeeded = false,
            Message = localizedMessage,
            StatusCode = ApiStatusCode.InternalServerError
        };
    }
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Success(string? message = null, ApiStatusCode statusCode = ApiStatusCode.Success, ILocalizationService? localizationService = null)
    {
        var localizedMessage = message ?? localizationService?.GetLocalizedString("OperationSuccessful") ?? "Operation completed successfully";
        return new ApiResponse
        {
            Succeeded = true,
            Message = localizedMessage,
            StatusCode = statusCode
        };
    }

    public static new ApiResponse Error(string? message = null, List<string>? errors = null, ApiStatusCode statusCode = ApiStatusCode.BadRequest, ILocalizationService? localizationService = null)
    {
        var localizedMessage = message ?? localizationService?.GetLocalizedString("ValidationError") ?? "An error occurred";
        return new ApiResponse
        {
            Succeeded = false,
            Message = localizedMessage,
            Errors = errors ?? new List<string>(),
            StatusCode = statusCode
        };
    }

    public static new ApiResponse Error(string? message = null, string? error = null, ApiStatusCode statusCode = ApiStatusCode.BadRequest, ILocalizationService? localizationService = null)
    {
        var localizedMessage = message ?? localizationService?.GetLocalizedString("ValidationError") ?? "An error occurred";
        return new ApiResponse
        {
            Succeeded = false,
            Message = localizedMessage,
            Errors = error != null ? new List<string> { error } : new List<string>(),
            StatusCode = statusCode
        };
    }

    public static new ApiResponse NotFound(string? message = null, ILocalizationService? localizationService = null)
    {
        var localizedMessage = message ?? localizationService?.GetLocalizedString("ResourceNotFound") ?? "Resource not found";
        return new ApiResponse
        {
            Succeeded = false,
            Message = localizedMessage,
            StatusCode = ApiStatusCode.NotFound
        };
    }

    public static new ApiResponse Unauthorized(string? message = null, ILocalizationService? localizationService = null)
    {
        var localizedMessage = message ?? localizationService?.GetLocalizedString("UnauthorizedAccess") ?? "Unauthorized access";
        return new ApiResponse
        {
            Succeeded = false,
            Message = localizedMessage,
            StatusCode = ApiStatusCode.Unauthorized
        };
    }

    public static new ApiResponse Forbidden(string? message = null, ILocalizationService? localizationService = null)
    {
        var localizedMessage = message ?? localizationService?.GetLocalizedString("AccessForbidden") ?? "Access forbidden";
        return new ApiResponse
        {
            Succeeded = false,
            Message = localizedMessage,
            StatusCode = ApiStatusCode.Forbidden
        };
    }

    public static new ApiResponse InternalServerError(string? message = null, ILocalizationService? localizationService = null)
    {
        var localizedMessage = message ?? localizationService?.GetLocalizedString("InternalServerError") ?? "Internal server error";
        return new ApiResponse
        {
            Succeeded = false,
            Message = localizedMessage,
            StatusCode = ApiStatusCode.InternalServerError
        };
    }
} 