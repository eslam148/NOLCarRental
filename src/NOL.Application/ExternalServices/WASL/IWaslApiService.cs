using Refit;

namespace NOL.Application.ExternalServices.WASL;

/// <summary>
/// WASL (Saudi Arabia Fleet Management) API Service
/// Documentation: https://wasl.api.elm.sa/
/// </summary>
public interface IWaslApiService
{
    /// <summary>
    /// Register a new vehicle in WASL system
    /// </summary>
    [Post("/vehicles/register")]
    Task<WaslResponse<VehicleRegistrationResponse>> RegisterVehicleAsync(
        [Body] VehicleRegistrationRequest request,
        [Header("Authorization")] string authorization);

    /// <summary>
    /// Update vehicle information
    /// </summary>
    [Put("/vehicles/{vehicleId}")]
    Task<WaslResponse<VehicleUpdateResponse>> UpdateVehicleAsync(
        string vehicleId,
        [Body] VehicleUpdateRequest request,
        [Header("Authorization")] string authorization);

    /// <summary>
    /// Delete/Unregister a vehicle from WASL
    /// </summary>
    [Delete("/vehicles/{vehicleId}")]
    Task<WaslResponse<object>> DeleteVehicleAsync(
        string vehicleId,
        [Header("Authorization")] string authorization);

    /// <summary>
    /// Send vehicle location update
    /// </summary>
    [Post("/locations")]
    Task<WaslResponse<LocationResponse>> SendLocationAsync(
        [Body] LocationUpdateRequest request,
        [Header("Authorization")] string authorization);

    /// <summary>
    /// Send batch location updates (multiple vehicles)
    /// </summary>
    [Post("/locations/batch")]
    Task<WaslResponse<BatchLocationResponse>> SendBatchLocationsAsync(
        [Body] BatchLocationRequest request,
        [Header("Authorization")] string authorization);

    /// <summary>
    /// Get vehicle status and information
    /// </summary>
    [Get("/vehicles/{vehicleId}")]
    Task<WaslResponse<VehicleInfo>> GetVehicleInfoAsync(
        string vehicleId,
        [Header("Authorization")] string authorization);

    /// <summary>
    /// Get all registered vehicles
    /// </summary>
    [Get("/vehicles")]
    Task<WaslResponse<List<VehicleInfo>>> GetAllVehiclesAsync(
        [Header("Authorization")] string authorization,
        [Query] int? page = null,
        [Query] int? pageSize = null);

    /// <summary>
    /// Register a new driver
    /// </summary>
    [Post("/drivers/register")]
    Task<WaslResponse<DriverRegistrationResponse>> RegisterDriverAsync(
        [Body] DriverRegistrationRequest request,
        [Header("Authorization")] string authorization);

    /// <summary>
    /// Update driver information
    /// </summary>
    [Put("/drivers/{driverId}")]
    Task<WaslResponse<DriverUpdateResponse>> UpdateDriverAsync(
        string driverId,
        [Body] DriverUpdateRequest request,
        [Header("Authorization")] string authorization);

    /// <summary>
    /// Get driver information
    /// </summary>
    [Get("/drivers/{driverId}")]
    Task<WaslResponse<DriverInfo>> GetDriverInfoAsync(
        string driverId,
        [Header("Authorization")] string authorization);

    /// <summary>
    /// Assign driver to vehicle
    /// </summary>
    [Post("/vehicles/{vehicleId}/assign-driver")]
    Task<WaslResponse<DriverAssignmentResponse>> AssignDriverToVehicleAsync(
        string vehicleId,
        [Body] DriverAssignmentRequest request,
        [Header("Authorization")] string authorization);

    /// <summary>
    /// Get vehicle trip history
    /// </summary>
    [Get("/vehicles/{vehicleId}/trips")]
    Task<WaslResponse<List<TripInfo>>> GetVehicleTripsAsync(
        string vehicleId,
        [Header("Authorization")] string authorization,
        [Query] DateTime? startDate = null,
        [Query] DateTime? endDate = null);

    /// <summary>
    /// Create a new rental contract
    /// </summary>
    [Post("/contracts/create")]
    Task<WaslResponse<CreateContractResponse>> CreateContractAsync(
        [Body] CreateContractRequest request,
        [Header("Authorization")] string authorization);

    /// <summary>
    /// Close/Complete a rental contract
    /// </summary>
    [Post("/contracts/close")]
    Task<WaslResponse<CloseContractResponse>> CloseContractAsync(
        [Body] CloseContractRequest request,
        [Header("Authorization")] string authorization);

    /// <summary>
    /// Extend rental contract end date
    /// </summary>
    [Put("/contracts/{contractId}/extend")]
    Task<WaslResponse<object>> ExtendContractAsync(
        string contractId,
        [Body] ExtendContractRequest request,
        [Header("Authorization")] string authorization);

    /// <summary>
    /// Get contract information and status
    /// </summary>
    [Get("/contracts/{contractId}")]
    Task<WaslResponse<ContractInfo>> GetContractInfoAsync(
        string contractId,
        [Header("Authorization")] string authorization);

    /// <summary>
    /// Get all contracts for a vehicle
    /// </summary>
    [Get("/vehicles/{vehicleId}/contracts")]
    Task<WaslResponse<List<ContractInfo>>> GetVehicleContractsAsync(
        string vehicleId,
        [Header("Authorization")] string authorization,
        [Query] string? status = null);

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [Get("/health")]
    Task<WaslHealthResponse> GetHealthAsync();
}

