using Microsoft.AspNetCore.Mvc;
using NOL.Application.Common.Interfaces;
using NOL.Application.Common.Responses;
using NOL.Application.DTOs;

namespace NOL.API.Controllers;

[Route("api/[controller]")]
public class BranchesController : ControllerBase
{
    private readonly IBranchService _branchService;

    public BranchesController(IBranchService branchService)
    {
        _branchService = branchService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedBranchesDto>>> GetBranches(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] decimal? latitude = null,
        [FromQuery] decimal? longitude = null,
        [FromQuery] double radiusKm = 50)
    {
        // If coordinates are provided, return nearby branches
        if (latitude.HasValue && longitude.HasValue)
        {
            var nearbyResult = await _branchService.GetBranchesNearbyAsync(latitude.Value, longitude.Value, radiusKm, page, pageSize);
            return StatusCode(nearbyResult.StatusCodeValue, nearbyResult);
        }

        // Otherwise, return all branches with pagination
        var result = await _branchService.GetBranchesPagedAsync(page, pageSize);
        return StatusCode(result.StatusCodeValue, result);
    }

    [HttpGet("all")]
    public async Task<ActionResult<ApiResponse<List<BranchDto>>>> GetAllBranches()
    {
        var result = await _branchService.GetBranchesAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<BranchDto>>> GetBranch(int id)
    {
        var result = await _branchService.GetBranchByIdAsync(id);
        return StatusCode(result.StatusCodeValue, result);
    }

    [HttpGet("country/{country}")]
    public async Task<ActionResult<ApiResponse<List<BranchDto>>>> GetBranchesByCountry(string country)
    {
        var result = await _branchService.GetBranchesByCountryAsync(country);
        return StatusCode(result.StatusCodeValue, result);
    }

    [HttpGet("city/{city}")]
    public async Task<ActionResult<ApiResponse<List<BranchDto>>>> GetBranchesByCity(string city)
    {
        var result = await _branchService.GetBranchesByCityAsync(city);
        return StatusCode(result.StatusCodeValue, result);
    }
}