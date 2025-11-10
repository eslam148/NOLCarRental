using Microsoft.Extensions.Logging;

namespace NOL.Application.ExternalServices.WASL;

/// <summary>
/// Wrapper service for WASL API integration
/// Provides high-level methods for fleet management operations
/// </summary>
public interface IWaslService
{
    /// <summary>
    /// Check WASL API health status and authentication
    /// </summary>
    Task<bool> IsWaslApiHealthyAsync();

    // TODO: Add more methods after successful authentication testing
}

/// <summary>
/// Implementation of WASL service
/// </summary>
public class WaslService : IWaslService
{
    private readonly IWaslApiService _waslApiService;
    private readonly ILogger<WaslService> _logger;

    public WaslService(
        IWaslApiService waslApiService,
        ILogger<WaslService> logger)
    {
        _waslApiService = waslApiService;
        _logger = logger;
    }

    public async Task<bool> IsWaslApiHealthyAsync()
    {
        try
        {
            _logger.LogInformation("Checking WASL API health");
            
            var response = await _waslApiService.GetHealthAsync();
            
            var isHealthy = response.Status == "healthy" || 
                          response.Status == "ok" || 
                          response.Status == "Healthy" ||
                          response.Status == "OK";
            
            if (isHealthy)
            {
                _logger.LogInformation("WASL API is healthy. Status: {Status}", response.Status);
            }
            else
            {
                _logger.LogWarning("WASL API returned non-healthy status: {Status}", response.Status);
            }
            
            return isHealthy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking WASL API health: {Message}", ex.Message);
            return false;
        }
    }

    // TODO: Add more methods after successful health check
    // - RegisterCarAsync()
    // - SendCarLocationAsync()
    // - CreateRentalContractAsync()
    // - etc.
}

