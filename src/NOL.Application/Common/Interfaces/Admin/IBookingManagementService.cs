using NOL.Application.Common.Responses;
using NOL.Application.DTOs.Admin;
using NOL.Domain.Enums;

namespace NOL.Application.Common.Interfaces.Admin;

public interface IBookingManagementService
{
    // CRUD Operations
    Task<ApiResponse<AdminBookingDto>> GetBookingByIdAsync(int id);
    Task<ApiResponse<List<AdminBookingDto>>> GetBookingsAsync(BookingFilterDto filter);
    Task<ApiResponse<AdminBookingDto>> CreateBookingAsync(AdminCreateBookingDto createBookingDto, string adminId);
    Task<ApiResponse<AdminBookingDto>> UpdateBookingAsync(int id, ModifyBookingDto modifyBookingDto, string adminId);
    Task<ApiResponse> DeleteBookingAsync(int id, string adminId);
    
    // Status Management
    Task<ApiResponse<AdminBookingDto>> UpdateBookingStatusAsync(int id, UpdateBookingStatusDto statusDto, string adminId);
    Task<ApiResponse> BulkUpdateBookingStatusAsync(List<int> bookingIds, BookingStatus status, string adminId, string? notes = null);
    
    // Booking Workflow
    Task<ApiResponse<AdminBookingDto>> ConfirmBookingAsync(int id, string adminId);
    Task<ApiResponse<AdminBookingDto>> StartBookingAsync(int id, string adminId);
    Task<ApiResponse<AdminBookingDto>> CompleteBookingAsync(int id, string adminId);
    Task<ApiResponse<AdminBookingDto>> CancelBookingAsync(int id, string cancellationReason, string adminId);
    
    // Bulk Operations
    Task<ApiResponse> BulkOperationAsync(BulkBookingOperationDto operationDto, string adminId);
    
    // Analytics and Reporting
    Task<ApiResponse<BookingAnalyticsDto>> GetBookingAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null, int? branchId = null);
    Task<ApiResponse<BookingReportDto>> GenerateBookingReportAsync(BookingFilterDto filter);
    Task<ApiResponse<byte[]>> ExportBookingReportAsync(BookingFilterDto filter, string format = "excel");
    
    // Payment Management
    Task<ApiResponse<List<PaymentDetailDto>>> GetBookingPaymentsAsync(int bookingId);
    Task<ApiResponse<PaymentDetailDto>> AddBookingPaymentAsync(int bookingId, PaymentDetailDto paymentDto, string adminId);
    Task<ApiResponse<PaymentDetailDto>> UpdatePaymentStatusAsync(int paymentId, PaymentStatus status, string adminId);
    
    // Revenue Tracking
    Task<ApiResponse<decimal>> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null, int? branchId = null);
    Task<ApiResponse<List<MonthlyBookingStatsDto>>> GetMonthlyRevenueAsync(int year, int? branchId = null);
    Task<ApiResponse<List<DailyRevenueDto>>> GetDailyRevenueAsync(DateTime startDate, DateTime endDate, int? branchId = null);
    
    // Peak Time Analysis
    Task<ApiResponse<List<PeakTimeStatsDto>>> GetPeakTimesAnalysisAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<List<PopularCarStatsDto>>> GetPopularCarsAnalysisAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<List<BranchBookingStatsDto>>> GetBranchPerformanceAsync(DateTime? startDate = null, DateTime? endDate = null);
    
    // Customer Booking Management
    Task<ApiResponse<List<AdminBookingDto>>> GetCustomerBookingsAsync(string customerId, BookingFilterDto? filter = null);
    Task<ApiResponse<decimal>> GetCustomerTotalSpentAsync(string customerId, DateTime? startDate = null, DateTime? endDate = null);
    
    // Validation and Business Rules
    Task<ApiResponse<bool>> ValidateBookingDatesAsync(int carId, DateTime startDate, DateTime endDate, int? excludeBookingId = null);
    Task<ApiResponse<decimal>> CalculateBookingCostAsync(int carId, DateTime startDate, DateTime endDate, List<AdminBookingExtraDto>? extras = null);
    Task<ApiResponse<List<int>>> GetAvailableCarsAsync(DateTime startDate, DateTime endDate, int? branchId = null);
}
