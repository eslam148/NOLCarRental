using Microsoft.AspNetCore.Mvc;
using NOL.Application.Common.Interfaces;
using NOL.Application.Common.Responses;
using NOL.Application.DTOs;

namespace NOL.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BranchesController : ControllerBase
{
    private readonly IBranchService _branchService;

    public BranchesController(IBranchService branchService)
    {
        _branchService = branchService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<BranchDto>>>> GetBranches()
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