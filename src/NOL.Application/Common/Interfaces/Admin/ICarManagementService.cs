using NOL.Application.Common.Responses;
using NOL.Application.DTOs.Admin;
using NOL.Domain.Enums;

namespace NOL.Application.Common.Interfaces.Admin;

public interface ICarManagementService
{
    // CRUD Operations
    Task<ApiResponse<AdminCarDto>> GetCarByIdAsync(int id);
    Task<ApiResponse<List<AdminCarDto>>> GetCarsAsync(CarFilterDto filter);
    Task<ApiResponse<AdminCarDto>> CreateCarAsync(AdminCreateCarDto createCarDto, string adminId);
    Task<ApiResponse<AdminCarDto>> UpdateCarAsync(int id, AdminUpdateCarDto updateCarDto, string adminId);
    Task<ApiResponse> DeleteCarAsync(int id, string adminId);
    
    // Status Management
    Task<ApiResponse<AdminCarDto>> UpdateCarStatusAsync(int id, CarStatus status, string adminId, string? notes = null);
    Task<ApiResponse> BulkUpdateCarStatusAsync(List<int> carIds, CarStatus status, string adminId, string? notes = null);
    
    // Bulk Operations
    Task<ApiResponse> BulkDeleteCarsAsync(List<int> carIds, string adminId);
    Task<ApiResponse> BulkOperationAsync(BulkCarOperationDto operationDto, string adminId);
    
    // Import/Export
    Task<ApiResponse<List<string>>> ImportCarsAsync(List<CarImportDto> cars, string adminId);
    Task<ApiResponse<byte[]>> ExportCarsAsync(CarFilterDto filter, string format = "excel");
    Task<ApiResponse<byte[]>> GetCarTemplateAsync();
    
    // Analytics
    Task<ApiResponse<CarAnalyticsDto>> GetCarAnalyticsAsync(int carId, DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<List<CarAnalyticsDto>>> GetCarsAnalyticsAsync(CarFilterDto filter, DateTime? startDate = null, DateTime? endDate = null);
    
    // Maintenance
    Task<ApiResponse<List<CarMaintenanceRecordDto>>> GetCarMaintenanceHistoryAsync(int carId);
    Task<ApiResponse<CarMaintenanceRecordDto>> AddMaintenanceRecordAsync(int carId, CarMaintenanceRecordDto maintenanceRecord, string adminId);
    Task<ApiResponse<List<AdminCarDto>>> GetCarsNeedingMaintenanceAsync();
    
    // Image Management
    Task<ApiResponse<string>> UploadCarImageAsync(int carId, Stream imageStream, string fileName, string adminId);
    Task<ApiResponse> DeleteCarImageAsync(int carId, string adminId);
    
    // Validation
    Task<ApiResponse<bool>> ValidatePlateNumberAsync(string plateNumber, int? excludeCarId = null);
    Task<ApiResponse<List<string>>> ValidateCarImportAsync(List<CarImportDto> cars);
}
