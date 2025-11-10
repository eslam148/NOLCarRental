using System.Text.Json.Serialization;

namespace NOL.Application.ExternalServices.WASL;

#region Common Response Wrapper

/// <summary>
/// Standard WASL API response wrapper
/// </summary>
public class WaslResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("errors")]
    public List<string>? Errors { get; set; }

    [JsonPropertyName("errorCode")]
    public string? ErrorCode { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Health check response
/// </summary>
public class WaslHealthResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }
}

#endregion

#region Vehicle Models

/// <summary>
/// Vehicle registration request
/// </summary>
public class VehicleRegistrationRequest
{
    /// <summary>
    /// Plate number (Arabic or English)
    /// </summary>
    [JsonPropertyName("plateNumber")]
    public string PlateNumber { get; set; } = string.Empty;

    /// <summary>
    /// Vehicle sequence number
    /// </summary>
    [JsonPropertyName("sequenceNumber")]
    public string? SequenceNumber { get; set; }

    /// <summary>
    /// Vehicle type (1: Car, 2: Truck, 3: Motorcycle, etc.)
    /// </summary>
    [JsonPropertyName("vehicleType")]
    public int VehicleType { get; set; }

    /// <summary>
    /// Vehicle identification number (VIN/Chassis number)
    /// </summary>
    [JsonPropertyName("vehicleId")]
    public string VehicleId { get; set; } = string.Empty;

    /// <summary>
    /// Plate type (1: Private, 2: Public transport, 3: Commercial, etc.)
    /// </summary>
    [JsonPropertyName("plateType")]
    public int PlateType { get; set; }

    /// <summary>
    /// Company/Owner ID
    /// </summary>
    [JsonPropertyName("companyId")]
    public string CompanyId { get; set; } = string.Empty;

    /// <summary>
    /// Reference ID in your system
    /// </summary>
    [JsonPropertyName("referenceKey")]
    public string? ReferenceKey { get; set; }

    /// <summary>
    /// Additional vehicle information
    /// </summary>
    [JsonPropertyName("additionalInfo")]
    public VehicleAdditionalInfo? AdditionalInfo { get; set; }
}

/// <summary>
/// Vehicle registration response
/// </summary>
public class VehicleRegistrationResponse
{
    [JsonPropertyName("waslVehicleId")]
    public string WaslVehicleId { get; set; } = string.Empty;

    [JsonPropertyName("plateNumber")]
    public string PlateNumber { get; set; } = string.Empty;

    [JsonPropertyName("registrationDate")]
    public DateTime RegistrationDate { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Vehicle update request
/// </summary>
public class VehicleUpdateRequest
{
    [JsonPropertyName("plateNumber")]
    public string? PlateNumber { get; set; }

    [JsonPropertyName("vehicleType")]
    public int? VehicleType { get; set; }

    [JsonPropertyName("plateType")]
    public int? PlateType { get; set; }

    [JsonPropertyName("additionalInfo")]
    public VehicleAdditionalInfo? AdditionalInfo { get; set; }
}

/// <summary>
/// Vehicle update response
/// </summary>
public class VehicleUpdateResponse
{
    [JsonPropertyName("waslVehicleId")]
    public string WaslVehicleId { get; set; } = string.Empty;

    [JsonPropertyName("updateDate")]
    public DateTime UpdateDate { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Vehicle information
/// </summary>
public class VehicleInfo
{
    [JsonPropertyName("waslVehicleId")]
    public string WaslVehicleId { get; set; } = string.Empty;

    [JsonPropertyName("plateNumber")]
    public string PlateNumber { get; set; } = string.Empty;

    [JsonPropertyName("sequenceNumber")]
    public string? SequenceNumber { get; set; }

    [JsonPropertyName("vehicleId")]
    public string VehicleId { get; set; } = string.Empty;

    [JsonPropertyName("vehicleType")]
    public int VehicleType { get; set; }

    [JsonPropertyName("plateType")]
    public int PlateType { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("registrationDate")]
    public DateTime RegistrationDate { get; set; }

    [JsonPropertyName("lastUpdate")]
    public DateTime? LastUpdate { get; set; }

    [JsonPropertyName("lastLocationUpdate")]
    public DateTime? LastLocationUpdate { get; set; }
}

/// <summary>
/// Additional vehicle information
/// </summary>
public class VehicleAdditionalInfo
{
    [JsonPropertyName("brand")]
    public string? Brand { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("year")]
    public int? Year { get; set; }

    [JsonPropertyName("color")]
    public string? Color { get; set; }

    [JsonPropertyName("ownerName")]
    public string? OwnerName { get; set; }
}

#endregion

#region Location Models

/// <summary>
/// Location update request
/// </summary>
public class LocationUpdateRequest
{
    /// <summary>
    /// Vehicle ID or plate number
    /// </summary>
    [JsonPropertyName("vehicleId")]
    public string VehicleId { get; set; } = string.Empty;

    /// <summary>
    /// Latitude
    /// </summary>
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    /// <summary>
    /// Longitude
    /// </summary>
    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    /// <summary>
    /// Speed in km/h
    /// </summary>
    [JsonPropertyName("speed")]
    public double? Speed { get; set; }

    /// <summary>
    /// Direction/Heading (0-360 degrees)
    /// </summary>
    [JsonPropertyName("heading")]
    public double? Heading { get; set; }

    /// <summary>
    /// GPS timestamp (when location was captured)
    /// </summary>
    [JsonPropertyName("gpsTimestamp")]
    public DateTime GpsTimestamp { get; set; }

    /// <summary>
    /// Odometer reading in KM
    /// </summary>
    [JsonPropertyName("odometer")]
    public double? Odometer { get; set; }

    /// <summary>
    /// Engine status (true: running, false: stopped)
    /// </summary>
    [JsonPropertyName("engineStatus")]
    public bool? EngineStatus { get; set; }

    /// <summary>
    /// Additional location data
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Location update response
/// </summary>
public class LocationResponse
{
    [JsonPropertyName("vehicleId")]
    public string VehicleId { get; set; } = string.Empty;

    [JsonPropertyName("accepted")]
    public bool Accepted { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Batch location request
/// </summary>
public class BatchLocationRequest
{
    [JsonPropertyName("locations")]
    public List<LocationUpdateRequest> Locations { get; set; } = new();
}

/// <summary>
/// Batch location response
/// </summary>
public class BatchLocationResponse
{
    [JsonPropertyName("totalProcessed")]
    public int TotalProcessed { get; set; }

    [JsonPropertyName("successCount")]
    public int SuccessCount { get; set; }

    [JsonPropertyName("failureCount")]
    public int FailureCount { get; set; }

    [JsonPropertyName("results")]
    public List<LocationResponse> Results { get; set; } = new();
}

#endregion

#region Driver Models

/// <summary>
/// Driver registration request
/// </summary>
public class DriverRegistrationRequest
{
    /// <summary>
    /// Driver's national ID or Iqama number
    /// </summary>
    [JsonPropertyName("identityNumber")]
    public string IdentityNumber { get; set; } = string.Empty;

    /// <summary>
    /// Driver's name in Arabic
    /// </summary>
    [JsonPropertyName("nameAr")]
    public string NameAr { get; set; } = string.Empty;

    /// <summary>
    /// Driver's name in English
    /// </summary>
    [JsonPropertyName("nameEn")]
    public string? NameEn { get; set; }

    /// <summary>
    /// Driver's phone number
    /// </summary>
    [JsonPropertyName("mobileNumber")]
    public string MobileNumber { get; set; } = string.Empty;

    /// <summary>
    /// Driver's email
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// Driving license number
    /// </summary>
    [JsonPropertyName("drivingLicenseNumber")]
    public string DrivingLicenseNumber { get; set; } = string.Empty;

    /// <summary>
    /// License expiry date
    /// </summary>
    [JsonPropertyName("licenseExpiryDate")]
    public DateTime LicenseExpiryDate { get; set; }

    /// <summary>
    /// Date of birth
    /// </summary>
    [JsonPropertyName("dateOfBirth")]
    public DateTime DateOfBirth { get; set; }

    /// <summary>
    /// Company ID
    /// </summary>
    [JsonPropertyName("companyId")]
    public string CompanyId { get; set; } = string.Empty;

    /// <summary>
    /// Reference ID in your system
    /// </summary>
    [JsonPropertyName("referenceKey")]
    public string? ReferenceKey { get; set; }
}

/// <summary>
/// Driver registration response
/// </summary>
public class DriverRegistrationResponse
{
    [JsonPropertyName("waslDriverId")]
    public string WaslDriverId { get; set; } = string.Empty;

    [JsonPropertyName("identityNumber")]
    public string IdentityNumber { get; set; } = string.Empty;

    [JsonPropertyName("registrationDate")]
    public DateTime RegistrationDate { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Driver update request
/// </summary>
public class DriverUpdateRequest
{
    [JsonPropertyName("nameAr")]
    public string? NameAr { get; set; }

    [JsonPropertyName("nameEn")]
    public string? NameEn { get; set; }

    [JsonPropertyName("mobileNumber")]
    public string? MobileNumber { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("drivingLicenseNumber")]
    public string? DrivingLicenseNumber { get; set; }

    [JsonPropertyName("licenseExpiryDate")]
    public DateTime? LicenseExpiryDate { get; set; }
}

/// <summary>
/// Driver update response
/// </summary>
public class DriverUpdateResponse
{
    [JsonPropertyName("waslDriverId")]
    public string WaslDriverId { get; set; } = string.Empty;

    [JsonPropertyName("updateDate")]
    public DateTime UpdateDate { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Driver information
/// </summary>
public class DriverInfo
{
    [JsonPropertyName("waslDriverId")]
    public string WaslDriverId { get; set; } = string.Empty;

    [JsonPropertyName("identityNumber")]
    public string IdentityNumber { get; set; } = string.Empty;

    [JsonPropertyName("nameAr")]
    public string NameAr { get; set; } = string.Empty;

    [JsonPropertyName("nameEn")]
    public string? NameEn { get; set; }

    [JsonPropertyName("mobileNumber")]
    public string MobileNumber { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("drivingLicenseNumber")]
    public string DrivingLicenseNumber { get; set; } = string.Empty;

    [JsonPropertyName("licenseExpiryDate")]
    public DateTime LicenseExpiryDate { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("registrationDate")]
    public DateTime RegistrationDate { get; set; }
}

/// <summary>
/// Driver assignment request
/// </summary>
public class DriverAssignmentRequest
{
    [JsonPropertyName("driverId")]
    public string DriverId { get; set; } = string.Empty;

    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// Driver assignment response
/// </summary>
public class DriverAssignmentResponse
{
    [JsonPropertyName("assignmentId")]
    public string AssignmentId { get; set; } = string.Empty;

    [JsonPropertyName("vehicleId")]
    public string VehicleId { get; set; } = string.Empty;

    [JsonPropertyName("driverId")]
    public string DriverId { get; set; } = string.Empty;

    [JsonPropertyName("assignmentDate")]
    public DateTime AssignmentDate { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

#endregion

#region Trip Models

/// <summary>
/// Trip information
/// </summary>
public class TripInfo
{
    [JsonPropertyName("tripId")]
    public string TripId { get; set; } = string.Empty;

    [JsonPropertyName("vehicleId")]
    public string VehicleId { get; set; } = string.Empty;

    [JsonPropertyName("driverId")]
    public string? DriverId { get; set; }

    [JsonPropertyName("startTime")]
    public DateTime StartTime { get; set; }

    [JsonPropertyName("endTime")]
    public DateTime? EndTime { get; set; }

    [JsonPropertyName("startLocation")]
    public LocationPoint? StartLocation { get; set; }

    [JsonPropertyName("endLocation")]
    public LocationPoint? EndLocation { get; set; }

    [JsonPropertyName("distanceKm")]
    public double? DistanceKm { get; set; }

    [JsonPropertyName("durationMinutes")]
    public int? DurationMinutes { get; set; }

    [JsonPropertyName("maxSpeed")]
    public double? MaxSpeed { get; set; }

    [JsonPropertyName("averageSpeed")]
    public double? AverageSpeed { get; set; }
}

/// <summary>
/// Geographic location point
/// </summary>
public class LocationPoint
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime? Timestamp { get; set; }
}

#endregion

#region Enums

/// <summary>
/// WASL vehicle types
/// </summary>
public enum WaslVehicleType
{
    /// <summary>
    /// سيارة خاصة - Private car
    /// </summary>
    PrivateCar = 1,

    /// <summary>
    /// شاحنة - Truck
    /// </summary>
    Truck = 2,

    /// <summary>
    /// دراجة نارية - Motorcycle
    /// </summary>
    Motorcycle = 3,

    /// <summary>
    /// حافلة - Bus
    /// </summary>
    Bus = 4,

    /// <summary>
    /// مركبة تجارية - Commercial vehicle
    /// </summary>
    CommercialVehicle = 5
}

/// <summary>
/// WASL plate types
/// </summary>
public enum WaslPlateType
{
    /// <summary>
    /// لوحة خاصة - Private plate
    /// </summary>
    Private = 1,

    /// <summary>
    /// نقل عام - Public transport
    /// </summary>
    PublicTransport = 2,

    /// <summary>
    /// تجاري - Commercial
    /// </summary>
    Commercial = 3,

    /// <summary>
    /// حكومي - Government
    /// </summary>
    Government = 4,

    /// <summary>
    /// دبلوماسي - Diplomatic
    /// </summary>
    Diplomatic = 5
}

/// <summary>
/// Vehicle status in WASL system
/// </summary>
public enum WaslVehicleStatus
{
    /// <summary>
    /// نشط - Active
    /// </summary>
    Active,

    /// <summary>
    /// معلق - Suspended
    /// </summary>
    Suspended,

    /// <summary>
    /// محذوف - Deleted
    /// </summary>
    Deleted,

    /// <summary>
    /// قيد المراجعة - Under review
    /// </summary>
    UnderReview
}

#endregion

