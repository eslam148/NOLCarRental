using Microsoft.AspNetCore.Mvc;
using NOL.Application.Common.Interfaces;
using NOL.Application.Common.Responses;
using NOL.Application.DTOs;
using NOL.Domain.Enums;

namespace NOL.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExtrasController : ControllerBase
{
    private readonly IExtraService _extraService;

    public ExtrasController(IExtraService extraService)
    {
        _extraService = extraService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ExtraDto>>>> GetExtras()
    {
        var result = await _extraService.GetExtrasAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ExtraDto>>> GetExtra(int id)
    {
        var result = await _extraService.GetExtraByIdAsync(id);
        return StatusCode(result.StatusCodeValue, result);
    }

    [HttpGet("active")]
    public async Task<ActionResult<ApiResponse<List<ExtraDto>>>> GetActiveExtras()
    {
        var result = await _extraService.GetActiveExtrasAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    [HttpGet("type/{type}")]
    public async Task<ActionResult<ApiResponse<List<ExtraDto>>>> GetExtrasByType(ExtraType type)
    {
        var result = await _extraService.GetExtrasByTypeAsync(type);
        return StatusCode(result.StatusCodeValue, result);
    }
} 