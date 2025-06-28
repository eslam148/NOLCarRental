using Microsoft.AspNetCore.Mvc;
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
[ApiController]
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
        [FromQuery] int pageSize = 10)
    {
        var result = await _carService.GetCarsAsync(sortByCost.ToString(), page, pageSize);
        return StatusCode(result.StatusCodeValue, result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CarDto>>> GetCar(int id)
    {
        var result = await _carService.GetCarByIdAsync(id);
        return StatusCode(result.StatusCodeValue, result);
    }

    [HttpGet("available")]
    public async Task<ActionResult<ApiResponse<List<CarDto>>>> GetAvailableCars(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var result = await _carService.GetAvailableCarsAsync(startDate, endDate);
        return StatusCode(result.StatusCodeValue, result);
    }

    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<ApiResponse<List<CarDto>>>> GetCarsByCategory(int categoryId)
    {
        var result = await _carService.GetCarsByCategoryAsync(categoryId);
        return StatusCode(result.StatusCodeValue, result);
    }

    [HttpGet("branch/{branchId}")]
    public async Task<ActionResult<ApiResponse<List<CarDto>>>> GetCarsByBranch(int branchId)
    {
        var result = await _carService.GetCarsByBranchAsync(branchId);
        return StatusCode(result.StatusCodeValue, result);
    }
} 