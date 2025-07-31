using System.ComponentModel.DataAnnotations;

namespace NOL.Application.DTOs.Admin;

public class DashboardStatsDto
{
    public OverallStatsDto OverallStats { get; set; } = new();
    public RevenueStatsDto RevenueStats { get; set; } = new();
    public BookingStatsDto BookingStats { get; set; } = new();
    public CarStatsDto CarStats { get; set; } = new();
    public CustomerStatsDto CustomerStats { get; set; } = new();
    public List<PopularCarDto> PopularCars { get; set; } = new();
    public List<RecentBookingDto> RecentBookings { get; set; } = new();
}

public class OverallStatsDto
{
    public int TotalBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public int ActiveCars { get; set; }
    public int TotalCustomers { get; set; }
    public int PendingBookings { get; set; }
    public int CompletedBookings { get; set; }
    public decimal AverageBookingValue { get; set; }
    public double CarUtilizationRate { get; set; }
}

public class RevenueStatsDto
{
    public decimal TodayRevenue { get; set; }
    public decimal WeekRevenue { get; set; }
    public decimal MonthRevenue { get; set; }
    public decimal YearRevenue { get; set; }
    public decimal PreviousMonthRevenue { get; set; }
    public double MonthlyGrowthPercentage { get; set; }
    public List<DailyRevenueDto> DailyRevenue { get; set; } = new();
    public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();
}

public class BookingStatsDto
{
    public int TodayBookings { get; set; }
    public int WeekBookings { get; set; }
    public int MonthBookings { get; set; }
    public int YearBookings { get; set; }
    public int CancelledBookings { get; set; }
    public double CancellationRate { get; set; }
    public List<BookingStatusCountDto> BookingsByStatus { get; set; } = new();
    public List<DailyBookingDto> DailyBookings { get; set; } = new();
}

public class CarStatsDto
{
    public int TotalCars { get; set; }
    public int AvailableCars { get; set; }
    public int RentedCars { get; set; }
    public int MaintenanceCars { get; set; }
    public int OutOfServiceCars { get; set; }
    public double UtilizationRate { get; set; }
    public List<CarCategoryStatsDto> CarsByCategory { get; set; } = new();
    public List<BranchCarStatsDto> CarsByBranch { get; set; } = new();
}

public class CustomerStatsDto
{
    public int TotalCustomers { get; set; }
    public int NewCustomersThisMonth { get; set; }
    public int ActiveCustomers { get; set; }
    public double CustomerRetentionRate { get; set; }
    public decimal AverageLoyaltyPoints { get; set; }
    public List<CustomerSegmentDto> CustomerSegments { get; set; } = new();
}

public class PopularCarDto
{
    public int CarId { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string PlateNumber { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
    public double UtilizationRate { get; set; }
}

public class RecentBookingDto
{
    public int BookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CarInfo { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class DailyRevenueDto
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
}

public class MonthlyRevenueDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
}

public class DailyBookingDto
{
    public DateTime Date { get; set; }
    public int BookingCount { get; set; }
    public int CompletedCount { get; set; }
    public int CancelledCount { get; set; }
}

public class BookingStatusCountDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class CarCategoryStatsDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int TotalCars { get; set; }
    public int AvailableCars { get; set; }
    public int RentedCars { get; set; }
    public double UtilizationRate { get; set; }
    public decimal Revenue { get; set; }
}

public class BranchCarStatsDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int TotalCars { get; set; }
    public int AvailableCars { get; set; }
    public int RentedCars { get; set; }
    public double UtilizationRate { get; set; }
    public decimal Revenue { get; set; }
}

public class CustomerSegmentDto
{
    public string SegmentName { get; set; } = string.Empty;
    public int CustomerCount { get; set; }
    public double Percentage { get; set; }
    public decimal AverageSpending { get; set; }
}

public class DashboardFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? BranchId { get; set; }
    public int? CategoryId { get; set; }
    public string? Period { get; set; } = "month"; // day, week, month, year
}
