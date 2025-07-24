using Microsoft.AspNetCore.Mvc;
using NOL.Application.Common.Interfaces;
using NOL.Application.Common.Services;
using System.Globalization;

namespace NOL.API.Controllers;

[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly LocalizedApiResponseService _responseService;
    private readonly ILocalizationService _localizationService;

    public TestController(LocalizedApiResponseService responseService, ILocalizationService localizationService)
    {
        _responseService = responseService;
        _localizationService = localizationService;
    }

    [HttpGet("localization")]
    public IActionResult TestLocalization([FromQuery] string? culture = null)
    {
        if (!string.IsNullOrEmpty(culture))
        {
            _localizationService.SetCulture(culture);
        }

        var currentCulture = CultureInfo.CurrentUICulture.Name;
        
        var testKeys = new[]
        {
            "PasswordsDoNotMatch",
            "PasswordRequired", 
            "ValidationError",
            "EmailAlreadyExists",
            "InvalidCredentials",
            "InvalidEmailORPassword"
        };

        var results = new Dictionary<string, object>();
        results["currentCulture"] = currentCulture;
        results["requestedCulture"] = culture ?? "none";
        
        foreach (var key in testKeys)
        {
            var value = _localizationService.GetLocalizedString(key);
            results[key] = value;
        }

        return Ok(results);
    }

    [HttpPost("password-validation")]
    public IActionResult TestPasswordValidation([FromQuery] string? culture = null)
    {
        if (!string.IsNullOrEmpty(culture))
        {
            _localizationService.SetCulture(culture);
        }

        // Using the NEW generic ValidationError method
        var response = _responseService.ValidationError<object>("PasswordsDoNotMatch");
        
        return BadRequest(response);
    }

    [HttpPost("multiple-validation")]
    public IActionResult TestMultipleValidationErrors([FromQuery] string? culture = null)
    {
        if (!string.IsNullOrEmpty(culture))
        {
            _localizationService.SetCulture(culture);
        }

        // Using the NEW generic ValidationError method with multiple errors
        var response = _responseService.ValidationError<object>(
            "PasswordsDoNotMatch", 
            "EmailAlreadyExists", 
            "InvalidEmailORPassword"
        );
        
        return BadRequest(response);
    }

    [HttpPost("dto-validation")]
    public IActionResult TestDtoValidationErrors([FromQuery] string? culture = null, [FromQuery] string type = "auth")
    {
        if (!string.IsNullOrEmpty(culture))
        {
            _localizationService.SetCulture(culture);
        }

        object response = type switch
        {
            "auth" => _responseService.ValidationError<object>(
                "PasswordsDoNotMatch", 
                "EmailAlreadyExists", 
                "InvalidEmailORPassword"
            ),
            "otp" => _responseService.ValidationError<object>(
                "OtpExpired", 
                "InvalidOtp", 
                "EmailAlreadyConfirmed"
            ),
            "dates" => _responseService.ValidationError<object>(
                "InvalidDateRange"
            ),
            _ => _responseService.ValidationError<object>("ValidationError")
        };
        
        return StatusCode(422, response);
    }

    [HttpPost("business-logic")]
    public IActionResult TestBusinessLogicErrors([FromQuery] string? culture = null, [FromQuery] string domain = "booking")
    {
        if (!string.IsNullOrEmpty(culture))
        {
            _localizationService.SetCulture(culture);
        }

        object response = domain switch
        {
            "booking" => _responseService.Error<object>(
                "CarNotAvailable", 
                new List<string> { 
                    _responseService.GetLocalizedString("CarNotAvailable"),
                    _responseService.GetLocalizedString("ReceivingBranchNotAvailable")
                }
            ),
            "loyalty" => _responseService.Error<object>(
                "MinimumRedemptionNotMet", 
                new List<string> { 
                    _responseService.GetLocalizedString("MinimumRedemptionNotMet"),
                    _responseService.GetLocalizedString("InsufficientLoyaltyPoints")
                }
            ),
            "advertisement" => _responseService.Error<object>(
                "CannotSetBothCarAndCategory", 
                new List<string> { 
                    _responseService.GetLocalizedString("CannotSetBothCarAndCategory"),
                    _responseService.GetLocalizedString("MustSetEitherCarOrCategory")
                }
            ),
            _ => _responseService.Error<object>("InternalServerError")
        };
        
        return BadRequest(response);
    }

    [HttpPost("login-validation-generic")]
    public IActionResult TestLoginValidationGeneric([FromQuery] string? culture = null, [FromQuery] string type = "invalid")
    {
        if (!string.IsNullOrEmpty(culture))
        {
            _localizationService.SetCulture(culture);
        }

        object response = type switch
        {
            "invalid" => _responseService.ValidationError<object>("InvalidEmailORPassword"),
            "unverified" => _responseService.ValidationError<object>("EmailNotVerified"),
            "multiple" => _responseService.ValidationError<object>("InvalidEmailORPassword", "EmailNotVerified"),
            _ => _responseService.ValidationError<object>("ValidationError")
        };
        
        return BadRequest(response);
    }

    [HttpPost("validation-status-codes")]
    public IActionResult TestValidationStatusCodes([FromQuery] string? culture = null, [FromQuery] int statusCode = 422)
    {
        if (!string.IsNullOrEmpty(culture))
        {
            _localizationService.SetCulture(culture);
        }

        var apiStatusCode = (NOL.Domain.Enums.ApiStatusCode)statusCode;
        var response = _responseService.ValidationError<object>(apiStatusCode, "PasswordsDoNotMatch", "InvalidEmailORPassword");
        
        return StatusCode(statusCode, response);
    }
} 