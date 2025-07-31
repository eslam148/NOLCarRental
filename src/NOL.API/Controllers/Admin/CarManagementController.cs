using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using NOL.Application.Common.Interfaces.Admin;
using NOL.Application.Common.Responses;
using NOL.Application.DTOs.Admin;
using NOL.Domain.Enums;

namespace NOL.API.Controllers.Admin;

/// <summary>
/// Car Management Controller - Admin operations for car fleet management
/// </summary>
[ApiController]
[Route("api/admin/cars")]
[Authorize(Roles = "Admin,SuperAdmin,BranchManager")]
[Tags("Admin Car Management")]
public class CarManagementController : ControllerBase
{
    private readonly ICarManagementService _carManagementService;

    public CarManagementController(ICarManagementService carManagementService)
    {
        _carManagementService = carManagementService;
    }

    /// <summary>
    /// Get all cars with advanced filtering and pagination
    /// </summary>
    /// <param name="filter">Car filter parameters</param>
    /// <returns>Paginated list of cars with admin details</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AdminCarDto>>>> GetCars([FromQuery] CarFilterDto filter)
    {
        var result = await _carManagementService.GetCarsAsync(filter);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get car by ID with detailed admin information
    /// </summary>
    /// <param name="id">Car ID</param>
    /// <returns>Car details with analytics</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<AdminCarDto>>> GetCarById(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid car ID" });
        }

        var result = await _carManagementService.GetCarByIdAsync(id);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Create a new car
    /// </summary>
    /// <param name="createCarDto">Car creation data</param>
    /// <returns>Created car details</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<AdminCarDto>>> CreateCar([FromBody] AdminCreateCarDto createCarDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _carManagementService.CreateCarAsync(createCarDto, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Update an existing car
    /// </summary>
    /// <param name="id">Car ID</param>
    /// <param name="updateCarDto">Car update data</param>
    /// <returns>Updated car details</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<AdminCarDto>>> UpdateCar(int id, [FromBody] AdminUpdateCarDto updateCarDto)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid car ID" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _carManagementService.UpdateCarAsync(id, updateCarDto, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Delete a car
    /// </summary>
    /// <param name="id">Car ID</param>
    /// <returns>Deletion confirmation</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteCar(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid car ID" });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _carManagementService.DeleteCarAsync(id, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Update car status
    /// </summary>
    /// <param name="id">Car ID</param>
    /// <param name="status">New car status</param>
    /// <param name="notes">Optional notes for status change</param>
    /// <returns>Updated car details</returns>
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<ApiResponse<AdminCarDto>>> UpdateCarStatus(int id, [FromBody] CarStatus status, [FromQuery] string? notes = null)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid car ID" });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _carManagementService.UpdateCarStatusAsync(id, status, adminId, notes);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Bulk update car status
    /// </summary>
    /// <param name="carIds">List of car IDs</param>
    /// <param name="status">New status for all cars</param>
    /// <param name="notes">Optional notes</param>
    /// <returns>Operation result</returns>
    [HttpPatch("bulk/status")]
    public async Task<ActionResult<ApiResponse>> BulkUpdateCarStatus([FromBody] List<int> carIds, [FromQuery] CarStatus status, [FromQuery] string? notes = null)
    {
        if (carIds == null || !carIds.Any())
        {
            return BadRequest(new { message = "Car IDs list cannot be empty" });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _carManagementService.BulkUpdateCarStatusAsync(carIds, status, adminId, notes);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Bulk delete cars
    /// </summary>
    /// <param name="carIds">List of car IDs to delete</param>
    /// <returns>Operation result</returns>
    [HttpDelete("bulk")]
    public async Task<ActionResult<ApiResponse>> BulkDeleteCars([FromBody] List<int> carIds)
    {
        if (carIds == null || !carIds.Any())
        {
            return BadRequest(new { message = "Car IDs list cannot be empty" });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _carManagementService.BulkDeleteCarsAsync(carIds, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Perform bulk operations on cars
    /// </summary>
    /// <param name="operationDto">Bulk operation details</param>
    /// <returns>Operation result</returns>
    [HttpPost("bulk/operation")]
    public async Task<ActionResult<ApiResponse>> BulkOperation([FromBody] BulkCarOperationDto operationDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _carManagementService.BulkOperationAsync(operationDto, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Import cars from file
    /// </summary>
    /// <param name="cars">List of cars to import</param>
    /// <returns>Import results with any errors</returns>
    [HttpPost("import")]
    public async Task<ActionResult<ApiResponse<List<string>>>> ImportCars([FromBody] List<CarImportDto> cars)
    {
        if (cars == null || !cars.Any())
        {
            return BadRequest(new { message = "Cars list cannot be empty" });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _carManagementService.ImportCarsAsync(cars, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Export cars to file
    /// </summary>
    /// <param name="filter">Filter for cars to export</param>
    /// <param name="format">Export format (excel, csv)</param>
    /// <returns>Export file</returns>
    [HttpPost("export")]
    public async Task<ActionResult> ExportCars([FromBody] CarFilterDto filter, [FromQuery] string format = "excel")
    {
        if (!new[] { "excel", "csv" }.Contains(format.ToLower()))
        {
            return BadRequest(new { message = "Format must be 'excel' or 'csv'" });
        }

        var result = await _carManagementService.ExportCarsAsync(filter, format.ToLower());
        
        if (!result.Succeeded)
        {
            return StatusCode(result.StatusCodeValue, result);
        }

        var contentType = format.ToLower() == "excel" ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" : "text/csv";
        var fileName = $"cars-export-{DateTime.UtcNow:yyyyMMdd-HHmmss}.{format.ToLower()}";

        return File(result.Data!, contentType, fileName);
    }

    /// <summary>
    /// Get car import template
    /// </summary>
    /// <returns>Excel template file for car import</returns>
    [HttpGet("import/template")]
    public async Task<ActionResult> GetCarImportTemplate()
    {
        var result = await _carManagementService.GetCarTemplateAsync();
        
        if (!result.Succeeded)
        {
            return StatusCode(result.StatusCodeValue, result);
        }

        return File(result.Data!, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "car-import-template.xlsx");
    }

    /// <summary>
    /// Get car analytics
    /// </summary>
    /// <param name="id">Car ID</param>
    /// <param name="startDate">Start date for analytics</param>
    /// <param name="endDate">End date for analytics</param>
    /// <returns>Car performance analytics</returns>
    [HttpGet("{id}/analytics")]
    public async Task<ActionResult<ApiResponse<CarAnalyticsDto>>> GetCarAnalytics(int id, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid car ID" });
        }

        var result = await _carManagementService.GetCarAnalyticsAsync(id, startDate, endDate);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get maintenance history for a car
    /// </summary>
    /// <param name="id">Car ID</param>
    /// <returns>List of maintenance records</returns>
    [HttpGet("{id}/maintenance")]
    public async Task<ActionResult<ApiResponse<List<CarMaintenanceRecordDto>>>> GetCarMaintenanceHistory(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid car ID" });
        }

        var result = await _carManagementService.GetCarMaintenanceHistoryAsync(id);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Add maintenance record for a car
    /// </summary>
    /// <param name="id">Car ID</param>
    /// <param name="maintenanceRecord">Maintenance record details</param>
    /// <returns>Created maintenance record</returns>
    [HttpPost("{id}/maintenance")]
    public async Task<ActionResult<ApiResponse<CarMaintenanceRecordDto>>> AddMaintenanceRecord(int id, [FromBody] CarMaintenanceRecordDto maintenanceRecord)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid car ID" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _carManagementService.AddMaintenanceRecordAsync(id, maintenanceRecord, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Get cars that need maintenance
    /// </summary>
    /// <returns>List of cars needing maintenance</returns>
    [HttpGet("maintenance/needed")]
    public async Task<ActionResult<ApiResponse<List<AdminCarDto>>>> GetCarsNeedingMaintenance()
    {
        var result = await _carManagementService.GetCarsNeedingMaintenanceAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Upload car image
    /// </summary>
    /// <param name="id">Car ID</param>
    /// <param name="image">Image file</param>
    /// <returns>Image URL</returns>
    [HttpPost("{id}/image")]
    public async Task<ActionResult<ApiResponse<string>>> UploadCarImage(int id, IFormFile image)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid car ID" });
        }

        if (image == null || image.Length == 0)
        {
            return BadRequest(new { message = "Image file is required" });
        }

        // Validate image file
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new { message = "Only JPG, PNG, and GIF files are allowed" });
        }

        if (image.Length > 5 * 1024 * 1024) // 5MB limit
        {
            return BadRequest(new { message = "Image file size cannot exceed 5MB" });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        using var stream = image.OpenReadStream();
        var result = await _carManagementService.UploadCarImageAsync(id, stream, image.FileName, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Delete car image
    /// </summary>
    /// <param name="id">Car ID</param>
    /// <returns>Operation result</returns>
    [HttpDelete("{id}/image")]
    public async Task<ActionResult<ApiResponse>> DeleteCarImage(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid car ID" });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId))
        {
            return Unauthorized();
        }

        var result = await _carManagementService.DeleteCarImageAsync(id, adminId);
        return StatusCode(result.StatusCodeValue, result);
    }

    /// <summary>
    /// Validate plate number uniqueness
    /// </summary>
    /// <param name="plateNumber">Plate number to validate</param>
    /// <param name="excludeCarId">Car ID to exclude from validation</param>
    /// <returns>Validation result</returns>
    [HttpGet("validate/plate-number")]
    public async Task<ActionResult<ApiResponse<bool>>> ValidatePlateNumber([FromQuery] string plateNumber, [FromQuery] int? excludeCarId = null)
    {
        if (string.IsNullOrWhiteSpace(plateNumber))
        {
            return BadRequest(new { message = "Plate number is required" });
        }

        var result = await _carManagementService.ValidatePlateNumberAsync(plateNumber, excludeCarId);
        return StatusCode(result.StatusCodeValue, result);
    }
}
