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
} 