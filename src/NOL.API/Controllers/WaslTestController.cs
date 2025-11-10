using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NOL.Application.Common.Interfaces;
using NOL.Application.ExternalServices.WASL;
using NOL.Domain.Entities;

namespace NOL.API.Controllers;

/// <summary>
/// Controller for testing WASL API integration
/// WASL = Saudi Arabia Fleet Management and Tracking System
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class WaslTestController : ControllerBase
{
    private readonly IWaslService _waslService;
    private readonly IWaslApiService _waslApiService;
    private readonly ICarRepository _carRepository;
    private readonly ILogger<WaslTestController> _logger;
    private readonly IConfiguration _configuration;

    public WaslTestController(
        IWaslService waslService,
        IWaslApiService waslApiService,
        ICarRepository carRepository,
        ILogger<WaslTestController> logger,
        IConfiguration configuration)
    {
        _waslService = waslService;
        _waslApiService = waslApiService;
        _carRepository = carRepository;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Check WASL API health and connectivity
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckHealth()
    {
        try
        {
            var isHealthy = await _waslService.IsWaslApiHealthyAsync();
            
            return Ok(new
            {
                success = true,
                waslApiHealthy = isHealthy,
                message = isHealthy ? "WASL API is healthy" : "WASL API is not responding",
                baseUrl = _configuration["ExternalApis:WASL:BaseUrl"],
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking WASL health");
            return StatusCode(500, new
            {
                success = false,
                message = "Error checking WASL health",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Register a car in WASL system
    /// </summary>
    [HttpPost("register-car/{carId}")]
    public async Task<IActionResult> RegisterCar(int carId)
    {
        try
        {
            var car = await _carRepository.GetByIdAsync(carId);
            if (car == null)
            {
                return NotFound(new { success = false, message = "Car not found" });
            }

            var companyId = _configuration["ExternalApis:WASL:CompanyId"] ?? "NOL-RENTAL-001";
            var result = await _waslService.RegisterCarAsync(car, companyId);

            if (result.Success && result.Data != null)
            {
                // Update car with WASL ID
                car.WaslVehicleId = result.Data.WaslVehicleId;
                await _carRepository.UpdateAsync(car);
            }

            return Ok(new
            {
                success = result.Success,
                message = result.Message,
                waslVehicleId = result.Data?.WaslVehicleId,
                carId = carId,
                plateNumber = car.PlateNumber,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering car {CarId} in WASL", carId);
            return StatusCode(500, new
            {
                success = false,
                message = "Error registering car in WASL",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Send location update for a car
    /// </summary>
    [HttpPost("send-location/{carId}")]
    public async Task<IActionResult> SendLocation(
        int carId,
        [FromBody] LocationTestRequest request)
    {
        try
        {
            var car = await _carRepository.GetByIdAsync(carId);
            if (car == null)
            {
                return NotFound(new { success = false, message = "Car not found" });
            }

            if (string.IsNullOrEmpty(car.WaslVehicleId))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Car is not registered in WASL. Please register it first."
                });
            }

            var result = await _waslService.SendCarLocationAsync(
                car.WaslVehicleId,
                request.Latitude,
                request.Longitude,
                request.Speed);

            return Ok(new
            {
                success = result.Success,
                message = result.Message,
                carId = carId,
                waslVehicleId = car.WaslVehicleId,
                location = new { request.Latitude, request.Longitude, request.Speed },
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending location for car {CarId}", carId);
            return StatusCode(500, new
            {
                success = false,
                message = "Error sending location to WASL",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get vehicle information from WASL
    /// </summary>
    [HttpGet("vehicle-info/{carId}")]
    public async Task<IActionResult> GetVehicleInfo(int carId)
    {
        try
        {
            var car = await _carRepository.GetByIdAsync(carId);
            if (car == null)
            {
                return NotFound(new { success = false, message = "Car not found" });
            }

            if (string.IsNullOrEmpty(car.WaslVehicleId))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Car is not registered in WASL"
                });
            }

            var apiKey = _configuration["ExternalApis:WASL:ApiKey"] ?? "";
            var authorization = $"Bearer {apiKey}";

            var result = await _waslApiService.GetVehicleInfoAsync(car.WaslVehicleId, authorization);

            return Ok(new
            {
                success = result.Success,
                message = result.Message,
                data = result.Data,
                carId = carId,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vehicle info for car {CarId}", carId);
            return StatusCode(500, new
            {
                success = false,
                message = "Error getting vehicle info from WASL",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Test registering a driver
    /// </summary>
    [HttpPost("register-driver")]
    public async Task<IActionResult> RegisterDriver([FromBody] DriverTestRequest request)
    {
        try
        {
            var driverRequest = new DriverRegistrationRequest
            {
                IdentityNumber = request.IdentityNumber,
                NameAr = request.NameAr,
                NameEn = request.NameEn,
                MobileNumber = request.MobileNumber,
                Email = request.Email,
                DrivingLicenseNumber = request.DrivingLicenseNumber,
                LicenseExpiryDate = request.LicenseExpiryDate,
                DateOfBirth = request.DateOfBirth,
                CompanyId = _configuration["ExternalApis:WASL:CompanyId"] ?? "NOL-RENTAL-001",
                ReferenceKey = request.ReferenceKey
            };

            var apiKey = _configuration["ExternalApis:WASL:ApiKey"] ?? "";
            var authorization = $"Bearer {apiKey}";

            var result = await _waslApiService.RegisterDriverAsync(driverRequest, authorization);

            return Ok(new
            {
                success = result.Success,
                message = result.Message,
                data = result.Data,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering driver in WASL");
            return StatusCode(500, new
            {
                success = false,
                message = "Error registering driver in WASL",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get WASL configuration info
    /// </summary>
    [HttpGet("config")]
    [AllowAnonymous]
    public IActionResult GetConfiguration()
    {
        var isEnabled = _configuration.GetValue<bool>("ExternalApis:WASL:Enabled");
        var baseUrl = _configuration["ExternalApis:WASL:BaseUrl"];
        var hasApiKey = !string.IsNullOrEmpty(_configuration["ExternalApis:WASL:ApiKey"]) &&
                        _configuration["ExternalApis:WASL:ApiKey"] != "YOUR_WASL_API_KEY_HERE";

        return Ok(new
        {
            enabled = isEnabled,
            baseUrl = baseUrl,
            hasValidApiKey = hasApiKey,
            message = isEnabled
                ? "WASL integration is enabled"
                : "WASL integration is disabled. Enable it in appsettings.json",
            endpoints = new
            {
                health = "/api/wasltest/health",
                registerCar = "/api/wasltest/register-car/{carId}",
                sendLocation = "/api/wasltest/send-location/{carId}",
                getVehicleInfo = "/api/wasltest/vehicle-info/{carId}",
                registerDriver = "/api/wasltest/register-driver"
            }
        });
    }
}

#region Request DTOs

public class LocationTestRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Speed { get; set; }
}

public class DriverTestRequest
{
    public string IdentityNumber { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string DrivingLicenseNumber { get; set; } = string.Empty;
    public DateTime LicenseExpiryDate { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? ReferenceKey { get; set; }
}

#endregion

