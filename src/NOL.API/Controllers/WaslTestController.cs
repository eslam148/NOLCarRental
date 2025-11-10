using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NOL.Application.ExternalServices.WASL;

namespace NOL.API.Controllers;

/// <summary>
/// Controller for testing WASL API authentication and connectivity
/// WASL = Saudi Arabia Fleet Management and Tracking System
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // For testing authentication - remove in production
public class WaslTestController : ControllerBase
{
    private readonly IWaslService _waslService;
    private readonly ILogger<WaslTestController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IWaslApiService _waslApiService;

    public WaslTestController(
        IWaslService waslService,
        ILogger<WaslTestController> logger,
        IConfiguration configuration, IWaslApiService waslApiService)
    {
        _waslService = waslService;
        _logger = logger;
        _configuration = configuration;
        _waslApiService = waslApiService;
    }

    #region  Health

    /// <summary>
    /// Check WASL API health and connectivity
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckHealth()
    {
        try
        {
            var clientId = _configuration["ExternalApis:WASL:ClientId"] ?? "";
            var appId = _configuration["ExternalApis:WASL:AppId"] ?? "";
            var appKey = _configuration["ExternalApis:WASL:AppKey"] ?? "";
            
            var hasClientId = !string.IsNullOrEmpty(clientId) && !clientId.StartsWith("YOUR_");
            var hasAppId = !string.IsNullOrEmpty(appId) && !appId.StartsWith("YOUR_");
            var hasAppKey = !string.IsNullOrEmpty(appKey) && !appKey.StartsWith("YOUR_");
            var isConfigured = hasClientId && hasAppId && hasAppKey;

            if (!isConfigured)
            {
                return Ok(new
                {
                    success = false,
                    waslApiHealthy = false,
                    message = "WASL credentials not fully configured",
                    missing = new
                    {
                        clientId = !hasClientId,
                        appId = !hasAppId,
                        appKey = !hasAppKey
                    },
                    note = "Please set client-id, app-id, and app-key in appsettings.json under ExternalApis:WASL",
                    baseUrl = _configuration["ExternalApis:WASL:BaseUrl"],
                    timestamp = DateTime.UtcNow
                });
            }

            var isHealthy = await _waslService.IsWaslApiHealthyAsync();
            
            return Ok(new
            {
                success = true,
                waslApiHealthy = isHealthy,
                message = isHealthy ? "✅ WASL API is healthy and authenticated" : "⚠️ WASL API responded but returned non-healthy status",
                baseUrl = _configuration["ExternalApis:WASL:BaseUrl"],
                authentication = new
                {
                    clientId = "Configured",
                    appId = "Configured",
                    appKey = "Configured"
                },
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking WASL health");
            return StatusCode(500, new
            {
                success = false,
                message = "❌ Error checking WASL health",
                error = ex.Message,
                errorType = ex.GetType().Name,
                possibleCauses = new[]
                {
                    "1. Invalid client-id, app-id, or app-key",
                    "2. Network connectivity issue",
                    "3. WASL API is down or unreachable",
                    "4. Credentials don't have proper permissions"
                }
            });
        }
    }

    #endregion


    #region Main Endpoints

    [HttpPost("CreateCar")]
    public async Task<IActionResult> CreateCarWasl()
    {
        var response = await _waslApiService.VehicleRegistration(new VehicleRegistrationRequest
        {

        });
        return Ok(response);
    }

    [HttpPost("UpdateCar")]
    public async Task<IActionResult> Locations()
    {
        var response = await _waslApiService.Location(new LocationRequest
        {

        });
        return Ok(response);
    }
    
    
    [HttpPost("RentalOperation")]
    public async Task<IActionResult> RentalOperation()
    {
        var response = await _waslApiService.RentalOperation(new RentalOperationRequest
        {

        });
        return Ok(response);
    }
    
    
    [HttpGet("VehicleEligibilityInquiryService")]
    public async Task<IActionResult> VehicleEligibilityInquiryService()
    {
        var response = await _waslApiService.VehicleEligibilityInquiryService("sss","234567");
        return Ok(response);
    }

    #endregion
 

    
    
    
}

