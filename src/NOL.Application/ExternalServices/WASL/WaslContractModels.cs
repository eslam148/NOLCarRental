using System.Text.Json.Serialization;

namespace NOL.Application.ExternalServices.WASL;

#region Rental Contract Models

/// <summary>
/// Create rental contract request
/// </summary>
public class CreateContractRequest
{
    /// <summary>
    /// Your internal contract/booking number
    /// </summary>
    [JsonPropertyName("contractNumber")]
    public string ContractNumber { get; set; } = string.Empty;

    /// <summary>
    /// WASL vehicle ID (from registration)
    /// </summary>
    [JsonPropertyName("vehicleId")]
    public string VehicleId { get; set; } = string.Empty;

    /// <summary>
    /// WASL driver ID (if driver registered) or customer ID
    /// </summary>
    [JsonPropertyName("driverId")]
    public string? DriverId { get; set; }

    /// <summary>
    /// Customer national ID or Iqama number
    /// </summary>
    [JsonPropertyName("customerId")]
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Customer full name in Arabic
    /// </summary>
    [JsonPropertyName("customerNameAr")]
    public string CustomerNameAr { get; set; } = string.Empty;

    /// <summary>
    /// Customer full name in English
    /// </summary>
    [JsonPropertyName("customerNameEn")]
    public string? CustomerNameEn { get; set; }

    /// <summary>
    /// Customer mobile number
    /// </summary>
    [JsonPropertyName("customerPhone")]
    public string CustomerPhone { get; set; } = string.Empty;

    /// <summary>
    /// Rental start date and time
    /// </summary>
    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Expected rental end date and time
    /// </summary>
    [JsonPropertyName("expectedEndDate")]
    public DateTime ExpectedEndDate { get; set; }

    /// <summary>
    /// Pickup branch location
    /// </summary>
    [JsonPropertyName("pickupLocation")]
    public LocationPoint? PickupLocation { get; set; }

    /// <summary>
    /// Expected return branch location
    /// </summary>
    [JsonPropertyName("returnLocation")]
    public LocationPoint? ReturnLocation { get; set; }

    /// <summary>
    /// Initial odometer reading (km)
    /// </summary>
    [JsonPropertyName("initialOdometer")]
    public double? InitialOdometer { get; set; }

    /// <summary>
    /// Company ID
    /// </summary>
    [JsonPropertyName("companyId")]
    public string CompanyId { get; set; } = string.Empty;

    /// <summary>
    /// Contract type (1: Daily, 2: Weekly, 3: Monthly)
    /// </summary>
    [JsonPropertyName("contractType")]
    public int ContractType { get; set; }

    /// <summary>
    /// Additional contract details
    /// </summary>
    [JsonPropertyName("additionalInfo")]
    public Dictionary<string, string>? AdditionalInfo { get; set; }
}

/// <summary>
/// Create contract response
/// </summary>
public class CreateContractResponse
{
    [JsonPropertyName("waslContractId")]
    public string WaslContractId { get; set; } = string.Empty;

    [JsonPropertyName("contractNumber")]
    public string ContractNumber { get; set; } = string.Empty;

    [JsonPropertyName("vehicleId")]
    public string VehicleId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Close rental contract request
/// </summary>
public class CloseContractRequest
{
    /// <summary>
    /// WASL contract ID or your contract number
    /// </summary>
    [JsonPropertyName("contractId")]
    public string ContractId { get; set; } = string.Empty;

    /// <summary>
    /// Actual return date and time
    /// </summary>
    [JsonPropertyName("actualEndDate")]
    public DateTime ActualEndDate { get; set; }

    /// <summary>
    /// Final odometer reading (km)
    /// </summary>
    [JsonPropertyName("finalOdometer")]
    public double? FinalOdometer { get; set; }

    /// <summary>
    /// Return location
    /// </summary>
    [JsonPropertyName("returnLocation")]
    public LocationPoint? ReturnLocation { get; set; }

    /// <summary>
    /// Contract closure reason (1: Normal, 2: Early return, 3: Extension)
    /// </summary>
    [JsonPropertyName("closureReason")]
    public int ClosureReason { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}

/// <summary>
/// Close contract response
/// </summary>
public class CloseContractResponse
{
    [JsonPropertyName("waslContractId")]
    public string WaslContractId { get; set; } = string.Empty;

    [JsonPropertyName("contractNumber")]
    public string ContractNumber { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("closedAt")]
    public DateTime ClosedAt { get; set; }

    [JsonPropertyName("totalDurationHours")]
    public double? TotalDurationHours { get; set; }

    [JsonPropertyName("totalDistanceKm")]
    public double? TotalDistanceKm { get; set; }
}

/// <summary>
/// Extend contract request
/// </summary>
public class ExtendContractRequest
{
    [JsonPropertyName("contractId")]
    public string ContractId { get; set; } = string.Empty;

    [JsonPropertyName("newExpectedEndDate")]
    public DateTime NewExpectedEndDate { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
}

/// <summary>
/// Contract information
/// </summary>
public class ContractInfo
{
    [JsonPropertyName("waslContractId")]
    public string WaslContractId { get; set; } = string.Empty;

    [JsonPropertyName("contractNumber")]
    public string ContractNumber { get; set; } = string.Empty;

    [JsonPropertyName("vehicleId")]
    public string VehicleId { get; set; } = string.Empty;

    [JsonPropertyName("driverId")]
    public string? DriverId { get; set; }

    [JsonPropertyName("customerId")]
    public string CustomerId { get; set; } = string.Empty;

    [JsonPropertyName("customerName")]
    public string CustomerName { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("expectedEndDate")]
    public DateTime ExpectedEndDate { get; set; }

    [JsonPropertyName("actualEndDate")]
    public DateTime? ActualEndDate { get; set; }

    [JsonPropertyName("initialOdometer")]
    public double? InitialOdometer { get; set; }

    [JsonPropertyName("finalOdometer")]
    public double? FinalOdometer { get; set; }

    [JsonPropertyName("totalDistanceKm")]
    public double? TotalDistanceKm { get; set; }

    [JsonPropertyName("violations")]
    public List<ViolationInfo>? Violations { get; set; }
}

/// <summary>
/// Violation information
/// </summary>
public class ViolationInfo
{
    [JsonPropertyName("violationId")]
    public string ViolationId { get; set; } = string.Empty;

    [JsonPropertyName("violationType")]
    public string ViolationType { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("location")]
    public LocationPoint? Location { get; set; }

    [JsonPropertyName("severity")]
    public string? Severity { get; set; }
}

#endregion

#region Enums

/// <summary>
/// Contract types
/// </summary>
public enum WaslContractType
{
    /// <summary>
    /// يومي - Daily rental
    /// </summary>
    Daily = 1,

    /// <summary>
    /// أسبوعي - Weekly rental
    /// </summary>
    Weekly = 2,

    /// <summary>
    /// شهري - Monthly rental
    /// </summary>
    Monthly = 3,

    /// <summary>
    /// طويل الأمد - Long-term rental
    /// </summary>
    LongTerm = 4
}

/// <summary>
/// Contract closure reasons
/// </summary>
public enum ContractClosureReason
{
    /// <summary>
    /// عادي - Normal completion
    /// </summary>
    Normal = 1,

    /// <summary>
    /// إرجاع مبكر - Early return
    /// </summary>
    EarlyReturn = 2,

    /// <summary>
    /// تمديد - Extended
    /// </summary>
    Extended = 3,

    /// <summary>
    /// ملغي - Cancelled
    /// </summary>
    Cancelled = 4
}

/// <summary>
/// Contract status
/// </summary>
public enum WaslContractStatus
{
    /// <summary>
    /// نشط - Active/Ongoing
    /// </summary>
    Active,

    /// <summary>
    /// مغلق - Closed
    /// </summary>
    Closed,

    /// <summary>
    /// ملغي - Cancelled
    /// </summary>
    Cancelled,

    /// <summary>
    /// منتهي - Expired
    /// </summary>
    Expired
}

#endregion

