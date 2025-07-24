using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NOL.Application.Common.Interfaces;
using NOL.Application.DTOs;
using NOL.Domain.Enums;
using System.Security.Claims;

namespace NOL.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager")]
public class CarManagementController : ControllerBase
{
    private readonly ICarService _carService;

    public CarManagementController(ICarService carService)
    {
        _carService = carService;
    }

    /// <summary>
    /// Create a new car (Admin/Manager only)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCar([FromBody] CreateCarDto createCarDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _carService.CreateCarAsync(createCarDto);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Update an existing car (Admin/Manager only)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCar(int id, [FromBody] UpdateCarDto updateCarDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _carService.UpdateCarAsync(id, updateCarDto);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Delete a car (soft delete) (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCar(int id)
    {
        var result = await _carService.DeleteCarAsync(id);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Update car status (Admin/Manager only)
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateCarStatus(int id, [FromBody] CarStatus status)
    {
        var result = await _carService.ToggleCarStatusAsync(id, status);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Update car rates (Admin/Manager only)
    /// </summary>
    [HttpPut("{id}/rates")]
    public async Task<IActionResult> UpdateCarRates(int id, [FromBody] UpdateCarRatesDto updateRatesDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _carService.UpdateCarRatesAsync(id, updateRatesDto);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get all car rates with pagination (Admin/Manager only)
    /// </summary>
    [HttpGet("rates")]
    public async Task<IActionResult> GetAllCarRates([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { message = "Invalid pagination parameters" });
        }

        var result = await _carService.GetAllCarRatesAsync(page, pageSize);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get specific car rates (Admin/Manager only)
    /// </summary>
    [HttpGet("{id}/rates")]
    public async Task<IActionResult> GetCarRates(int id)
    {
        var result = await _carService.GetCarRatesAsync(id);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Bulk update car rates (Admin/Manager only)
    /// </summary>
    [HttpPut("rates/bulk")]
    public async Task<IActionResult> BulkUpdateRates([FromBody] BulkUpdateRatesDto bulkUpdateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (bulkUpdateDto.CarRates == null || !bulkUpdateDto.CarRates.Any())
        {
            return BadRequest(new { message = "No car rates provided for update" });
        }

        var result = await _carService.BulkUpdateRatesAsync(bulkUpdateDto);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Search cars for management (Admin/Manager only)
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchCars([FromQuery] string searchTerm, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { message = "Invalid pagination parameters" });
        }

        var result = await _carService.SearchCarsAsync(searchTerm, page, pageSize);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Validate if car exists (Admin/Manager only)
    /// </summary>
    [HttpGet("{id}/validate")]
    public async Task<IActionResult> ValidateCarExists(int id)
    {
        var result = await _carService.ValidateCarExistsAsync(id);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get car management statistics (Admin only)
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetCarStatistics()
    {
        try
        {
            // This would typically be implemented as a separate service method
            // For now, returning a placeholder response
            var stats = new
            {
                TotalCars = 0, // TODO: Implement actual statistics
                AvailableCars = 0,
                RentedCars = 0,
                MaintenanceCars = 0,
                OutOfServiceCars = 0,
                AverageRates = new
                {
                    Daily = 0m,
                    Weekly = 0m,
                    Monthly = 0m
                }
            };

            return Ok(new { Succeeded = true, Data = stats, Message = "Statistics retrieved" });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Succeeded = false, Message = "Internal server error" });
        }
    }

    /// <summary>
    /// Export car data (Admin only)
    /// </summary>
    [HttpGet("export")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ExportCarData([FromQuery] string format = "json")
    {
        try
        {
            // This would typically export to CSV, Excel, etc.
            // For now, returning a placeholder response
            return Ok(new { Succeeded = true, Message = "Export functionality not yet implemented" });
        }
        catch (Exception)
        {
            return StatusCode(500, new { Succeeded = false, Message = "Internal server error" });
        }
    }
}
