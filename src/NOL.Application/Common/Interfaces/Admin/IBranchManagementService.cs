using NOL.Application.Common.Responses;
using NOL.Application.DTOs.Admin;
using NOL.Domain.Enums;

namespace NOL.Application.Common.Interfaces.Admin;

public interface IBranchManagementService
{
    #region CRUD Operations
    
    /// <summary>
    /// Get branch by ID with complete admin details
    /// </summary>
    Task<ApiResponse<AdminBranchDto>> GetBranchByIdAsync(int id);
    
    /// <summary>
    /// Get branches with advanced filtering, sorting, and pagination
    /// </summary>
    Task<ApiResponse<List<AdminBranchDto>>> GetBranchesAsync(BranchFilterDto filter);
    
    /// <summary>
    /// Create a new branch
    /// </summary>
    Task<ApiResponse<AdminBranchDto>> CreateBranchAsync(AdminCreateBranchDto createBranchDto, string adminId);
    
    /// <summary>
    /// Update branch information
    /// </summary>
    Task<ApiResponse<AdminBranchDto>> UpdateBranchAsync(int id, AdminUpdateBranchDto updateBranchDto, string adminId);
    
    /// <summary>
    /// Delete branch (soft delete with validation)
    /// </summary>
    Task<ApiResponse> DeleteBranchAsync(int id, string adminId);
    
    #endregion

    #region Branch Status Management
    
    /// <summary>
    /// Activate branch
    /// </summary>
    Task<ApiResponse<AdminBranchDto>> ActivateBranchAsync(int id, string adminId);
    
    /// <summary>
    /// Deactivate branch (with active bookings/cars validation)
    /// </summary>
    Task<ApiResponse<AdminBranchDto>> DeactivateBranchAsync(int id, string adminId);
    
    /// <summary>
    /// Bulk update branch status
    /// </summary>
    Task<ApiResponse> BulkUpdateBranchStatusAsync(List<int> branchIds, bool isActive, string adminId);
    
    #endregion

    #region Branch Analytics
    
    /// <summary>
    /// Get comprehensive branch analytics
    /// </summary>
    Task<ApiResponse<BranchAnalyticsDto>> GetBranchAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    
    /// <summary>
    /// Get top performing branches
    /// </summary>
    Task<ApiResponse<List<BranchPerformanceDto>>> GetTopPerformingBranchesAsync(int count = 10, DateTime? startDate = null, DateTime? endDate = null);
    
    /// <summary>
    /// Get low performing branches
    /// </summary>
    Task<ApiResponse<List<BranchPerformanceDto>>> GetLowPerformingBranchesAsync(int count = 10, DateTime? startDate = null, DateTime? endDate = null);
    
    /// <summary>
    /// Get branch performance comparison
    /// </summary>
    Task<ApiResponse<BranchComparisonResultDto>> CompareBranchPerformanceAsync(BranchComparisonDto comparisonDto);
    
    /// <summary>
    /// Get branch revenue analysis
    /// </summary>
    Task<ApiResponse<BranchRevenueAnalysisDto>> GetBranchRevenueAnalysisAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null);
    
    #endregion

    #region Staff Management
    
    /// <summary>
    /// Get branch staff members
    /// </summary>
    Task<ApiResponse<List<BranchStaffDto>>> GetBranchStaffAsync(int branchId);
    
    /// <summary>
    /// Assign staff to branch
    /// </summary>
    Task<ApiResponse> AssignStaffToBranchAsync(BranchStaffAssignmentDto assignmentDto, string adminId);
    
    /// <summary>
    /// Remove staff from branch
    /// </summary>
    Task<ApiResponse> RemoveStaffFromBranchAsync(int branchId, List<string> staffIds, string adminId);
    
    /// <summary>
    /// Get staff assigned to multiple branches
    /// </summary>
    Task<ApiResponse<List<BranchStaffDto>>> GetMultiBranchStaffAsync();
    
    #endregion

    #region Car Fleet Management
    
    /// <summary>
    /// Get cars assigned to branch
    /// </summary>
    Task<ApiResponse<List<AdminCarDto>>> GetBranchCarsAsync(int branchId, CarStatus? status = null);
    
    /// <summary>
    /// Transfer cars between branches
    /// </summary>
    Task<ApiResponse> TransferCarsBetweenBranchesAsync(BranchCarTransferDto transferDto, string adminId);
    
    /// <summary>
    /// Get branch car utilization statistics
    /// </summary>
    Task<ApiResponse<BranchCarStatsDto>> GetBranchCarUtilizationAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null);
    
    #endregion

    #region Booking Management
    
    /// <summary>
    /// Get branch booking statistics
    /// </summary>
    Task<ApiResponse<BranchBookingStatsDto>> GetBranchBookingStatsAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null);
    
    /// <summary>
    /// Get branches by booking volume
    /// </summary>
    Task<ApiResponse<List<BranchPerformanceDto>>> GetBranchesByBookingVolumeAsync(DateTime? startDate = null, DateTime? endDate = null);
    
    #endregion

    #region Maintenance Management
    
    /// <summary>
    /// Schedule branch maintenance
    /// </summary>
    Task<ApiResponse> ScheduleBranchMaintenanceAsync(BranchMaintenanceScheduleDto scheduleDto, string adminId);
    
    /// <summary>
    /// Get branch maintenance history
    /// </summary>
    Task<ApiResponse<List<BranchMaintenanceScheduleDto>>> GetBranchMaintenanceHistoryAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null);
    
    /// <summary>
    /// Get upcoming maintenance schedules
    /// </summary>
    Task<ApiResponse<List<BranchMaintenanceScheduleDto>>> GetUpcomingMaintenanceAsync(DateTime? fromDate = null, int days = 30);
    
    #endregion

    #region Geographic Analytics
    
    /// <summary>
    /// Get branch statistics by city
    /// </summary>
    Task<ApiResponse<List<BranchCityStatsDto>>> GetBranchStatsByCityAsync();
    
    /// <summary>
    /// Get branch statistics by country
    /// </summary>
    Task<ApiResponse<List<BranchCountryStatsDto>>> GetBranchStatsByCountryAsync();
    
    /// <summary>
    /// Get branches within radius of location
    /// </summary>
    Task<ApiResponse<List<AdminBranchDto>>> GetBranchesNearLocationAsync(decimal latitude, decimal longitude, double radiusKm = 50);
    
    #endregion

    #region Bulk Operations
    
    /// <summary>
    /// Perform bulk operations on branches
    /// </summary>
    Task<ApiResponse> BulkOperationAsync(BulkBranchOperationDto operationDto, string adminId);
    
    /// <summary>
    /// Bulk delete branches
    /// </summary>
    Task<ApiResponse> BulkDeleteBranchesAsync(List<int> branchIds, string adminId);
    
    #endregion

    #region Reports and Export
    
    /// <summary>
    /// Generate comprehensive branch report
    /// </summary>
    Task<ApiResponse<BranchReportDto>> GenerateBranchReportAsync(BranchFilterDto filter);
    
    /// <summary>
    /// Export branch report to file
    /// </summary>
    Task<ApiResponse<byte[]>> ExportBranchReportAsync(BranchFilterDto filter, string format = "excel");
    
    /// <summary>
    /// Export branch performance metrics
    /// </summary>
    Task<ApiResponse<byte[]>> ExportBranchPerformanceAsync(DateTime? startDate = null, DateTime? endDate = null, string format = "excel");
    
    #endregion

    #region Validation and Search
    
    /// <summary>
    /// Validate branch location (coordinates, address)
    /// </summary>
    Task<ApiResponse<bool>> ValidateBranchLocationAsync(decimal latitude, decimal longitude, string address);
    
    /// <summary>
    /// Check if branch name is unique
    /// </summary>
    Task<ApiResponse<bool>> ValidateBranchNameAsync(string nameEn, string nameAr, int? excludeBranchId = null);
    
    /// <summary>
    /// Search branches by multiple criteria
    /// </summary>
    Task<ApiResponse<List<AdminBranchDto>>> SearchBranchesAsync(string searchTerm, int page = 1, int pageSize = 10);
    
    /// <summary>
    /// Get active branches only
    /// </summary>
    Task<ApiResponse<List<AdminBranchDto>>> GetActiveBranchesAsync();
    
    /// <summary>
    /// Get branches by geographic region
    /// </summary>
    Task<ApiResponse<List<AdminBranchDto>>> GetBranchesByRegionAsync(string city, string country);
    
    #endregion

    #region Performance Monitoring
    
    /// <summary>
    /// Get branch performance score
    /// </summary>
    Task<ApiResponse<double>> GetBranchPerformanceScoreAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null);
    
    /// <summary>
    /// Get branch efficiency metrics
    /// </summary>
    Task<ApiResponse<BranchPerformanceDto>> GetBranchEfficiencyMetricsAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null);
    
    /// <summary>
    /// Get monthly branch statistics
    /// </summary>
    Task<ApiResponse<List<MonthlyBranchStatsDto>>> GetMonthlyBranchStatsAsync(int year);
    
    #endregion
}
