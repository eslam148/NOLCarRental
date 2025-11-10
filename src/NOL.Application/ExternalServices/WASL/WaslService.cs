using Microsoft.Extensions.Logging;
using NOL.Domain.Entities;
using NOL.Domain.Enums;

namespace NOL.Application.ExternalServices.WASL;

/// <summary>
/// Wrapper service for WASL API integration
/// Provides high-level methods for fleet management operations
/// </summary>
public interface IWaslService
{
    /// <summary>
    /// Register a car in WASL system
    /// </summary>
    Task<WaslResponse<VehicleRegistrationResponse>> RegisterCarAsync(Car car, string companyId);

    /// <summary>
    /// Update car information in WASL
    /// </summary>
    Task<WaslResponse<VehicleUpdateResponse>> UpdateCarAsync(string waslVehicleId, Car car);

    /// <summary>
    /// Send car location update to WASL
    /// </summary>
    Task<WaslResponse<LocationResponse>> SendCarLocationAsync(string waslVehicleId, double latitude, double longitude, double? speed = null);

    /// <summary>
    /// Check WASL API health status
    /// </summary>
    Task<bool> IsWaslApiHealthyAsync();

    /// <summary>
    /// Create rental contract in WASL when booking starts
    /// </summary>
    Task<WaslResponse<CreateContractResponse>> CreateRentalContractAsync(
        Booking booking, 
        string customerId, 
        string customerNameAr, 
        string customerPhone);

    /// <summary>
    /// Close rental contract in WASL when booking ends
    /// </summary>
    Task<WaslResponse<CloseContractResponse>> CloseRentalContractAsync(
        string waslContractId, 
        DateTime returnDate, 
        double? finalOdometer = null);
}

/// <summary>
/// Implementation of WASL service
/// </summary>
public class WaslService : IWaslService
{
    private readonly IWaslApiService _waslApiService;
    private readonly ILogger<WaslService> _logger;
    private readonly string _apiKey;

    public WaslService(
        IWaslApiService waslApiService,
        ILogger<WaslService> logger,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _waslApiService = waslApiService;
        _logger = logger;
        _apiKey = configuration["ExternalApis:WASL:ApiKey"] ?? string.Empty;
    }

    public async Task<WaslResponse<VehicleRegistrationResponse>> RegisterCarAsync(Car car, string companyId)
    {
        try
        {
            _logger.LogInformation("Registering car {CarId} in WASL system", car.Id);

            var request = new VehicleRegistrationRequest
            {
                PlateNumber = car.PlateNumber ?? $"TEMP-{car.Id}",
                SequenceNumber = car.SequenceNumber,
                VehicleType = MapToWaslVehicleType(car),
                VehicleId = car.ChassisNumber ?? car.Id.ToString(),
                PlateType = (int)WaslPlateType.Commercial,
                CompanyId = companyId,
                ReferenceKey = car.Id.ToString(),
                AdditionalInfo = new VehicleAdditionalInfo
                {
                    Brand = car.BrandEn,
                    Model = car.ModelEn,
                    Year = car.Year,
                    Color = car.ColorEn,
                    OwnerName = "NOL Car Rental"
                }
            };

            var authorization = $"Bearer {_apiKey}";
            var response = await _waslApiService.RegisterVehicleAsync(request, authorization);

            if (response.Success)
            {
                _logger.LogInformation("Car {CarId} registered successfully in WASL. WASL ID: {WaslId}",
                    car.Id, response.Data?.WaslVehicleId);
            }
            else
            {
                _logger.LogWarning("Failed to register car {CarId} in WASL: {Message}",
                    car.Id, response.Message);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering car {CarId} in WASL", car.Id);
            throw;
        }
    }

    public async Task<WaslResponse<VehicleUpdateResponse>> UpdateCarAsync(string waslVehicleId, Car car)
    {
        try
        {
            _logger.LogInformation("Updating car {CarId} in WASL system. WASL ID: {WaslId}",
                car.Id, waslVehicleId);

            var request = new VehicleUpdateRequest
            {
                PlateNumber = car.PlateNumber,
                VehicleType = MapToWaslVehicleType(car),
                PlateType = (int)WaslPlateType.Commercial,
                AdditionalInfo = new VehicleAdditionalInfo
                {
                    Brand = car.BrandEn,
                    Model = car.ModelEn,
                    Year = car.Year,
                    Color = car.ColorEn
                }
            };

            var authorization = $"Bearer {_apiKey}";
            var response = await _waslApiService.UpdateVehicleAsync(waslVehicleId, request, authorization);

            if (response.Success)
            {
                _logger.LogInformation("Car {CarId} updated successfully in WASL", car.Id);
            }
            else
            {
                _logger.LogWarning("Failed to update car {CarId} in WASL: {Message}",
                    car.Id, response.Message);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating car {CarId} in WASL", car.Id);
            throw;
        }
    }

    public async Task<WaslResponse<LocationResponse>> SendCarLocationAsync(
        string waslVehicleId, 
        double latitude, 
        double longitude, 
        double? speed = null)
    {
        try
        {
            var request = new LocationUpdateRequest
            {
                VehicleId = waslVehicleId,
                Latitude = latitude,
                Longitude = longitude,
                Speed = speed,
                GpsTimestamp = DateTime.UtcNow,
                EngineStatus = true
            };

            var authorization = $"Bearer {_apiKey}";
            var response = await _waslApiService.SendLocationAsync(request, authorization);

            if (!response.Success)
            {
                _logger.LogWarning("Failed to send location for WASL vehicle {WaslId}: {Message}",
                    waslVehicleId, response.Message);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending location for WASL vehicle {WaslId}", waslVehicleId);
            throw;
        }
    }

    public async Task<bool> IsWaslApiHealthyAsync()
    {
        try
        {
            var response = await _waslApiService.GetHealthAsync();
            return response.Status == "healthy" || response.Status == "ok";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking WASL API health");
            return false;
        }
    }

    public async Task<WaslResponse<CreateContractResponse>> CreateRentalContractAsync(
        Booking booking,
        string customerId,
        string customerNameAr,
        string customerPhone)
    {
        try
        {
            _logger.LogInformation("Creating WASL rental contract for booking {BookingId}", booking.Id);

            if (string.IsNullOrEmpty(booking.Car.WaslVehicleId))
            {
                _logger.LogWarning("Car {CarId} is not registered in WASL. Cannot create contract.", booking.Car.Id);
                return new WaslResponse<CreateContractResponse>
                {
                    Success = false,
                    Message = "Car is not registered in WASL",
                    Errors = new List<string> { "Register the car in WASL first" },
                    Timestamp = DateTime.UtcNow
                };
            }

            var contractType = (booking.EndDate - booking.StartDate).Days switch
            {
                >= 30 => (int)WaslContractType.Monthly,
                >= 7 => (int)WaslContractType.Weekly,
                _ => (int)WaslContractType.Daily
            };

            var request = new CreateContractRequest
            {
                ContractNumber = booking.BookingNumber,
                VehicleId = booking.Car.WaslVehicleId,
                CustomerId = customerId,
                CustomerNameAr = customerNameAr,
                CustomerNameEn = booking.User?.FullName,
                CustomerPhone = customerPhone,
                StartDate = booking.StartDate,
                ExpectedEndDate = booking.EndDate,
                PickupLocation = new LocationPoint
                {
                    Latitude = (double)booking.ReceivingBranch.Latitude,
                    Longitude = (double)booking.ReceivingBranch.Longitude,
                    Address = booking.ReceivingBranch.Address
                },
                ReturnLocation = new LocationPoint
                {
                    Latitude = (double)booking.DeliveryBranch.Latitude,
                    Longitude = (double)booking.DeliveryBranch.Longitude,
                    Address = booking.DeliveryBranch.Address
                },
                CompanyId = _apiKey, // Or get from config
                ContractType = contractType,
                AdditionalInfo = new Dictionary<string, string>
                {
                    { "BookingId", booking.Id.ToString() },
                    { "TotalCost", booking.FinalAmount.ToString("F2") },
                    { "ReceivingBranch", booking.ReceivingBranch.NameEn ?? "" },
                    { "DeliveryBranch", booking.DeliveryBranch.NameEn ?? "" }
                }
            };

            var authorization = $"Bearer {_apiKey}";
            var response = await _waslApiService.CreateContractAsync(request, authorization);

            if (response.Success)
            {
                _logger.LogInformation("WASL contract created successfully for booking {BookingId}. WASL Contract ID: {ContractId}",
                    booking.Id, response.Data?.WaslContractId);
            }
            else
            {
                _logger.LogWarning("Failed to create WASL contract for booking {BookingId}: {Message}",
                    booking.Id, response.Message);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating WASL contract for booking {BookingId}", booking.Id);
            throw;
        }
    }

    public async Task<WaslResponse<CloseContractResponse>> CloseRentalContractAsync(
        string waslContractId,
        DateTime returnDate,
        double? finalOdometer = null)
    {
        try
        {
            _logger.LogInformation("Closing WASL contract {ContractId}", waslContractId);

            var request = new CloseContractRequest
            {
                ContractId = waslContractId,
                ActualEndDate = returnDate,
                FinalOdometer = finalOdometer,
                ClosureReason = (int)ContractClosureReason.Normal
            };

            var authorization = $"Bearer {_apiKey}";
            var response = await _waslApiService.CloseContractAsync(request, authorization);

            if (response.Success)
            {
                _logger.LogInformation("WASL contract {ContractId} closed successfully", waslContractId);
            }
            else
            {
                _logger.LogWarning("Failed to close WASL contract {ContractId}: {Message}",
                    waslContractId, response.Message);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing WASL contract {ContractId}", waslContractId);
            throw;
        }
    }

    /// <summary>
    /// Map Car entity to WASL vehicle type
    /// </summary>
    private int MapToWaslVehicleType(Car car)
    {
        // Default to Private Car for now
        // You can enhance this based on car category or other properties
        return (int)WaslVehicleType.PrivateCar;
    }
}

