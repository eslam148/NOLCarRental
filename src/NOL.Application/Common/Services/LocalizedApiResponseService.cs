using NOL.Domain.Enums;
using NOL.Application.Common.Interfaces;
using NOL.Application.Common.Responses;

namespace NOL.Application.Common.Services;

public class LocalizedApiResponseService
{
    private readonly ILocalizationService _localizationService;

    public LocalizedApiResponseService(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    public ApiResponse<T> Success<T>(T data, string messageKey = "OperationSuccessful", params object[] args)
    {
        var message = _localizationService.GetLocalizedString(messageKey, args);
        return ApiResponse<T>.Success(data, message);
    }

    public ApiResponse<T> Error<T>(string messageKey, List<string>? errors = null, ApiStatusCode statusCode = ApiStatusCode.BadRequest, params object[] args)
    {
        var message = _localizationService.GetLocalizedString(messageKey, args);
        return ApiResponse<T>.Error(message, errors, statusCode);
    }

    public ApiResponse<T> NotFound<T>(string messageKey = "ResourceNotFound", params object[] args)
    {
        var message = _localizationService.GetLocalizedString(messageKey, args);
        return ApiResponse<T>.NotFound(message);
    }

    public ApiResponse<T> Unauthorized<T>(string messageKey = "UnauthorizedAccess", params object[] args)
    {
        var message = _localizationService.GetLocalizedString(messageKey, args);
        return ApiResponse<T>.Unauthorized(message);
    }

    public ApiResponse<T> Forbidden<T>(string messageKey = "AccessForbidden", params object[] args)
    {
        var message = _localizationService.GetLocalizedString(messageKey, args);
        return ApiResponse<T>.Forbidden(message);
    }

    // Non-generic methods for ApiResponse (without data)
    public ApiResponse Success(string messageKey = "OperationSuccessful", params object[] args)
    {
        var message = _localizationService.GetLocalizedString(messageKey, args);
        return ApiResponse.Success(message);
    }

    public ApiResponse Error(string messageKey, List<string>? errors = null, ApiStatusCode statusCode = ApiStatusCode.BadRequest, params object[] args)
    {
        var message = _localizationService.GetLocalizedString(messageKey, args);
        return ApiResponse.Error(message, errors, statusCode);
    }

    public ApiResponse NotFound(string messageKey = "ResourceNotFound", params object[] args)
    {
        var message = _localizationService.GetLocalizedString(messageKey, args);
        return ApiResponse.NotFound(message);
    }

    public ApiResponse Unauthorized(string messageKey = "UnauthorizedAccess", params object[] args)
    {
        var message = _localizationService.GetLocalizedString(messageKey, args);
        return ApiResponse.Unauthorized(message);
    }

    public ApiResponse Forbidden(string messageKey = "AccessForbidden", params object[] args)
    {
        var message = _localizationService.GetLocalizedString(messageKey, args);
        return ApiResponse.Forbidden(message);
    }

    // Direct access to localization for complex scenarios
    public string GetLocalizedString(string key, params object[] args)
    {
        return _localizationService.GetLocalizedString(key, args);
    }

    // Generic Validation Error Response Methods
    public ApiResponse<T> ValidationError<T>(params string[] validationErrorKeys)
    {
        return ValidationError<T>(ApiStatusCode.UnprocessableEntity, validationErrorKeys);
    }

    public ApiResponse<T> ValidationError<T>(ApiStatusCode statusCode, params string[] validationErrorKeys)
    {
        var errors = validationErrorKeys.Select(key => _localizationService.GetLocalizedString(key)).ToList();
        var mainMessage = _localizationService.GetLocalizedString("ValidationError");
        return ApiResponse<T>.Error(mainMessage, errors, statusCode);
    }

    public ApiResponse<T> ValidationError<T>(List<string> validationErrorKeys)
    {
        return ValidationError<T>(ApiStatusCode.UnprocessableEntity, validationErrorKeys);
    }

    public ApiResponse<T> ValidationError<T>(ApiStatusCode statusCode, List<string> validationErrorKeys)
    {
        var errors = validationErrorKeys.Select(key => _localizationService.GetLocalizedString(key)).ToList();
        var mainMessage = _localizationService.GetLocalizedString("ValidationError");
        return ApiResponse<T>.Error(mainMessage, errors, statusCode);
    }

    public ApiResponse ValidationError(params string[] validationErrorKeys)
    {
        return ValidationError(ApiStatusCode.UnprocessableEntity, validationErrorKeys);
    }

    public ApiResponse ValidationError(ApiStatusCode statusCode, params string[] validationErrorKeys)
    {
        var errors = validationErrorKeys.Select(key => _localizationService.GetLocalizedString(key)).ToList();
        var mainMessage = _localizationService.GetLocalizedString("ValidationError");
        return ApiResponse.Error(mainMessage, errors, statusCode);
    }

    public ApiResponse ValidationError(List<string> validationErrorKeys)
    {
        return ValidationError(ApiStatusCode.UnprocessableEntity, validationErrorKeys);
    }

    public ApiResponse ValidationError(ApiStatusCode statusCode, List<string> validationErrorKeys)
    {
        var errors = validationErrorKeys.Select(key => _localizationService.GetLocalizedString(key)).ToList();
        var mainMessage = _localizationService.GetLocalizedString("ValidationError");
        return ApiResponse.Error(mainMessage, errors, statusCode);
    }
} 