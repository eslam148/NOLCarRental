using NOL.Application.Common.Responses;
using NOL.Application.DTOs.Admin;

namespace NOL.Application.Common.Interfaces.Admin;

public interface IDashboardService
{
    // Dashboard Statistics
    Task<ApiResponse<DashboardStatsDto>> GetDashboardStatsAsync(DashboardFilterDto filter);
    Task<ApiResponse<OverallStatsDto>> GetOverallStatsAsync(DashboardFilterDto filter);
    Task<ApiResponse<RevenueStatsDto>> GetRevenueStatsAsync(DashboardFilterDto filter);
    Task<ApiResponse<BookingStatsDto>> GetBookingStatsAsync(DashboardFilterDto filter);
    Task<ApiResponse<CarStatsDto>> GetCarStatsAsync(DashboardFilterDto filter);
    Task<ApiResponse<CustomerStatsDto>> GetCustomerStatsAsync(DashboardFilterDto filter);
    
    // Real-time Data
    Task<ApiResponse<List<RecentBookingDto>>> GetRecentBookingsAsync(int count = 10);
    Task<ApiResponse<List<PopularCarDto>>> GetPopularCarsAsync(int count = 10, DashboardFilterDto? filter = null);
    
    // Analytics Export
    Task<ApiResponse<byte[]>> ExportDashboardReportAsync(DashboardFilterDto filter, string format = "pdf");
    Task<ApiResponse<string>> GetDashboardReportUrlAsync(DashboardFilterDto filter, string format = "pdf");
}
