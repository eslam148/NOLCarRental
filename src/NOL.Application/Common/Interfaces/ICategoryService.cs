using NOL.Application.Common.Responses;
using NOL.Application.DTOs;

namespace NOL.Application.Common.Interfaces;

public interface ICategoryService
{
    Task<ApiResponse<List<CategoryDto>>> GetCategoriesAsync();
    Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(int id);
    Task<ApiResponse<List<CategoryDto>>> GetActiveCategoriesAsync();
} 