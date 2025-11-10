using Refit;

namespace NOL.Application.ExternalServices.WASL;

/// <summary>
/// WASL (Saudi Arabia Fleet Management) API Service
/// Documentation: https://wasl.api.elm.sa/
/// Authentication: Requires client-id, app-id, and app-key headers (automatically added)
/// </summary>
public interface IWaslApiService
{
    /// <summary>
    /// Health check endpoint to verify authentication and connectivity
    /// </summary>
    [Get("/")]
    Task<WaslHealthResponse> GetHealthAsync();

    
    [Post("/api/eRental/v1/vehicles")]
    Task<WaslResponse<VehicleRegistrationResponse>> VehicleRegistration([Body] VehicleRegistrationRequest request);

    [Get("/api/eRental/v1/inquiry/vehicles?plate={arabicPlate}&sequenceNumber={number}")]
    Task<WaslResponse<VehicleEligibilityRespone>> VehicleEligibilityInquiryService(string arabicPlate, string number);
    
    [Post("/api/eRental/v1/rental-operation")]
    Task<WaslResponse<RentalOperationResponse>> RentalOperation([Body] RentalOperationRequest request);
    
    
    [Post("/api/eRental/v1/locations")]
    Task<WaslResponse<LocationResposne>> Location([Body] LocationRequest request);
  
}

