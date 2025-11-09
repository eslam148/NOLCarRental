using NOL.Domain.Enums;
using NOL.Application.Common.Interfaces;
using NOL.Application.Common.Responses;
using NOL.Domain.Extensions;

namespace NOL.Application.Common.Services;

public class LocalizedApiResponseService
{
    private readonly ILocalizationService _localizationService;

    public LocalizedApiResponseService(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    public ApiResponse<T> Success<T>(T data, ResponseCode messageKey , params object[] args)
    {
        var message = messageKey.GetDescription();
        return ApiResponse<T>.Success(data, message);
    }

    public ApiResponse<T> Error<T>(ResponseCode messageKey, List<string>? errors = null, ApiStatusCode statusCode = ApiStatusCode.BadRequest, params object[] args)
    {
        var message = messageKey.GetDescription();
        return ApiResponse<T>.Error(message, errors, statusCode);
    }

    public ApiResponse<T> NotFound<T>(ResponseCode messageKey , params object[] args)
    {
        var message = messageKey.GetDescription();
        return ApiResponse<T>.NotFound(message);
    }

 
    public ApiResponse<T> Forbidden<T>(ResponseCode messageKey, params object[] args)
    {
        var message = messageKey.GetDescription();
        return ApiResponse<T>.Forbidden(message);
    }

    // Non-generic methods for ApiResponse (without data)
    public ApiResponse Success(ResponseCode messageKey , params object[] args)
    {
        var message = messageKey.GetDescription();
        return ApiResponse.Success(message);
    }

    public ApiResponse Error(ResponseCode messageKey, List<string>? errors = null, ApiStatusCode statusCode = ApiStatusCode.BadRequest, params object[] args)
    {
        var message = messageKey.GetDescription();
        return ApiResponse.Error(message, errors, statusCode);
    }

    public ApiResponse NotFound(ResponseCode messageKey , params object[] args)
    {
        var message = messageKey.GetDescription();
        return ApiResponse.NotFound(message);
    }

    public ApiResponse Unauthorized(ResponseCode messageKey , params object[] args)
    {
        var message = messageKey.GetDescription();
        return ApiResponse.Unauthorized(message);
    }

    public ApiResponse Forbidden(ResponseCode messageKey , params object[] args)
    {
        var message = messageKey.GetDescription();
        return ApiResponse.Forbidden(message);
    }

    

    // Generic Validation Error Response Methods
    public ApiResponse<T> ValidationError<T>(params ResponseCode[] validationErrorKeys)
    {
         return ValidationError<T>(ApiStatusCode.UnprocessableEntity, validationErrorKeys);
    }

    public ApiResponse<T> ValidationError<T>(ApiStatusCode statusCode, params ResponseCode[] validationErrorKeys)
    {
        var errors = validationErrorKeys.Select(key => key.GetDescription()).ToList();
        var mainMessage = ResponseCode.ValidationError.GetDescription();
        return ApiResponse<T>.Error(mainMessage, errors, statusCode);
    }

    public ApiResponse<T> ValidationError<T>(List<ResponseCode> validationErrorKeys)
    {
        return ValidationError<T>(ApiStatusCode.UnprocessableEntity, validationErrorKeys);
    }

    public ApiResponse<T> ValidationError<T>(ApiStatusCode statusCode, List<ResponseCode> validationErrorKeys)
    {
        var errors = validationErrorKeys.Select(key => key.GetDescription()).ToList();
        var mainMessage = ResponseCode.ValidationError.GetDescription();
        return ApiResponse<T>.Error(mainMessage, errors, statusCode);
    }

    public ApiResponse ValidationError(params ResponseCode[] validationErrorKeys)
    {
        return ValidationError(ApiStatusCode.UnprocessableEntity, validationErrorKeys);
    }

    public ApiResponse ValidationError(ApiStatusCode statusCode, params ResponseCode[] validationErrorKeys)
    {
        var errors = validationErrorKeys.Select(key => key.GetDescription()).ToList();
        var mainMessage = ResponseCode.ValidationError.GetDescription();
        return ApiResponse.Error(mainMessage, errors, statusCode);
    }

    public ApiResponse ValidationError(List<ResponseCode> validationErrorKeys)
    {
        return ValidationError(ApiStatusCode.UnprocessableEntity, validationErrorKeys);
    }

    public ApiResponse ValidationError(ApiStatusCode statusCode, List<ResponseCode> validationErrorKeys)
    {
        var errors = validationErrorKeys.Select(key => key.GetDescription()).ToList();
        var mainMessage = ResponseCode.ValidationError.GetDescription();
        return ApiResponse.Error(mainMessage, errors, statusCode);
    }
} 