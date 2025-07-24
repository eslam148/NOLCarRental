using NOL.Application.Common.Responses;
using NOL.Application.DTOs;
using NOL.Domain.Enums;

namespace NOL.Application.Common.Interfaces;

public interface ICarService
{
    // Read operations
    Task<ApiResponse<List<CarDto>>> GetCarsAsync(string? sortByCost = null, int page = 1, int pageSize = 10, string? brand = null, string? userId = null);
    Task<ApiResponse<CarDto>> GetCarByIdAsync(int id, string? userId = null);
    Task<ApiResponse<List<CarDto>>> GetAvailableCarsAsync(DateTime startDate, DateTime endDate, string? userId = null);
    Task<ApiResponse<List<CarDto>>> GetCarsByCategoryAsync(int categoryId, string? userId = null);
    Task<ApiResponse<List<CarDto>>> GetCarsByBranchAsync(int branchId, string? userId = null);

    // Car management operations (Admin only)
    Task<ApiResponse<CarDto>> CreateCarAsync(CreateCarDto createCarDto);
    Task<ApiResponse<CarDto>> UpdateCarAsync(int id, UpdateCarDto updateCarDto);
    Task<ApiResponse<bool>> DeleteCarAsync(int id);
    Task<ApiResponse<bool>> ToggleCarStatusAsync(int id, CarStatus status);

    // Rate management operations (Admin only)
    Task<ApiResponse<CarRatesDto>> UpdateCarRatesAsync(int id, UpdateCarRatesDto updateRatesDto);
    Task<ApiResponse<List<CarRatesDto>>> GetAllCarRatesAsync(int page = 1, int pageSize = 10);
    Task<ApiResponse<CarRatesDto>> GetCarRatesAsync(int id);
    Task<ApiResponse<List<CarRatesDto>>> BulkUpdateRatesAsync(BulkUpdateRatesDto bulkUpdateDto);

    // Utility operations
    Task<ApiResponse<List<CarDto>>> SearchCarsAsync(string searchTerm, int page = 1, int pageSize = 10);
    Task<ApiResponse<bool>> ValidateCarExistsAsync(int id);
}