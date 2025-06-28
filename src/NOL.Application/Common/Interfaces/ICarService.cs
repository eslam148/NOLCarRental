using NOL.Application.Common.Responses;
using NOL.Application.DTOs;
using NOL.Domain.Enums;

namespace NOL.Application.Common.Interfaces;

public interface ICarService
{
    Task<ApiResponse<List<CarDto>>> GetCarsAsync(CarStatus? status = null, int? categoryId = null, int page = 1, int pageSize = 10);
    Task<ApiResponse<CarDto>> GetCarByIdAsync(int id);
    Task<ApiResponse<List<CarDto>>> GetAvailableCarsAsync(DateTime startDate, DateTime endDate);
    Task<ApiResponse<List<CarDto>>> GetCarsByCategoryAsync(int categoryId);
    Task<ApiResponse<List<CarDto>>> GetCarsByBranchAsync(int branchId);
} 