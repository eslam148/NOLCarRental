using Microsoft.AspNetCore.Mvc;
using NOL.Application.Common.Interfaces;
using NOL.Application.Common.Responses;
using NOL.Application.DTOs;

namespace NOL.API.Controllers;

[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetCategories()
    {
        var result = await _categoryService.GetCategoriesAsync();
        return StatusCode(result.StatusCodeValue, result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetCategory(int id)
    {
        var result = await _categoryService.GetCategoryByIdAsync(id);
        return StatusCode(result.StatusCodeValue, result);
    }

    [HttpGet("active")]
    public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetActiveCategories()
    {
        var result = await _categoryService.GetActiveCategoriesAsync();
        return StatusCode(result.StatusCodeValue, result);
    }
} 