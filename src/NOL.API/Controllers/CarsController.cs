using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using NOL.Application.Common.Interfaces;
using NOL.Application.Common.Responses;
using NOL.Application.DTOs;
using NOL.Domain.Enums;

namespace NOL.API.Controllers;
public enum sortCar
{
    asc,
    desc
}
[Route("api/[controller]")]
public class CarsController : ControllerBase
{
    private readonly ICarService _carService;

    public CarsController(ICarService carService)
    {
        _carService = carService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CarDto>>>> GetCars(
        [FromQuery] sortCar sortByCost = sortCar.asc, // "asc" or "desc"
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? brand = null) // Filter by brand name
    {
        // Get user ID from authentication context (null if not authenticated)
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var result = await _carService.GetCarsAsync(sortByCost.ToString(), page, pageSize, brand, userId);
        return StatusCode(result.StatusCodeValue, result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CarDto>>> GetCarById(int id)
    {
        // Get user ID from authentication context (null if not authenticated)
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var result = await _carService.GetCarByIdAsync(id, userId);
        return StatusCode(result.StatusCodeValue, result);
    }

    [HttpGet("available")]
    public async Task<ActionResult<ApiResponse<List<CarDto>>>> GetAvailableCars(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        // Get user ID from authentication context (null if not authenticated)
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var result = await _carService.GetAvailableCarsAsync(startDate, endDate, userId);
        return StatusCode(result.StatusCodeValue, result);
    }

    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<ApiResponse<List<CarDto>>>> GetCarsByCategory(int categoryId)
    {
        // Get user ID from authentication context (null if not authenticated)
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var result = await _carService.GetCarsByCategoryAsync(categoryId, userId);
        return StatusCode(result.StatusCodeValue, result);
    }

    [HttpGet("branch/{branchId}")]
    public async Task<ActionResult<ApiResponse<List<CarDto>>>> GetCarsByBranch(int branchId)
    {
        // Get user ID from authentication context (null if not authenticated)
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var result = await _carService.GetCarsByBranchAsync(branchId, userId);
        return StatusCode(result.StatusCodeValue, result);
    }
} 