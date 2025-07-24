using NOL.Application.Common.Responses;
using NOL.Application.DTOs;
using NOL.Domain.Enums;

namespace NOL.Application.Common.Interfaces;

public interface ICarService
{
    Task<ApiResponse<List<CarDto>>> GetCarsAsync(string? sortByCost = null, int page = 1, int pageSize = 10, string? brand = null, string? userId = null);
    Task<ApiResponse<CarDto>> GetCarByIdAsync(int id, string? userId = null);
    Task<ApiResponse<List<CarDto>>> GetAvailableCarsAsync(DateTime startDate, DateTime endDate, string? userId = null);
    Task<ApiResponse<List<CarDto>>> GetCarsByCategoryAsync(int categoryId, string? userId = null);
    Task<ApiResponse<List<CarDto>>> GetCarsByBranchAsync(int branchId, string? userId = null);
} 