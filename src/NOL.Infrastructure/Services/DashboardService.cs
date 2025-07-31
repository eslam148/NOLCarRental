using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NOL.Application.Common.Interfaces.Admin;
using NOL.Application.Common.Responses;
using NOL.Application.DTOs.Admin;
using NOL.Domain.Entities;
using NOL.Domain.Enums;
using NOL.Infrastructure.Data;

namespace NOL.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<DashboardService> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    #region Dashboard Statistics

    public async Task<ApiResponse<DashboardStatsDto>> GetDashboardStatsAsync(DashboardFilterDto filter)
    {
        try
        {
            var overallStats = await GetOverallStatsAsync(filter);
            var revenueStats = await GetRevenueStatsAsync(filter);
            var bookingStats = await GetBookingStatsAsync(filter);
            var carStats = await GetCarStatsAsync(filter);
            var customerStats = await GetCustomerStatsAsync(filter);
            var popularCars = await GetPopularCarsAsync(10, filter);
            var recentBookings = await GetRecentBookingsAsync(10);

            var dashboardStats = new DashboardStatsDto
            {
                OverallStats = overallStats.Data ?? new OverallStatsDto(),
                RevenueStats = revenueStats.Data ?? new RevenueStatsDto(),
                BookingStats = bookingStats.Data ?? new BookingStatsDto(),
                CarStats = carStats.Data ?? new CarStatsDto(),
                CustomerStats = customerStats.Data ?? new CustomerStatsDto(),
                PopularCars = popularCars.Data ?? new List<PopularCarDto>(),
                RecentBookings = recentBookings.Data ?? new List<RecentBookingDto>()
            };

            return ApiResponse<DashboardStatsDto>.Success(dashboardStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard stats");
            return ApiResponse<DashboardStatsDto>.Error("An error occurred while retrieving dashboard statistics", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<OverallStatsDto>> GetOverallStatsAsync(DashboardFilterDto filter)
    {
        try
        {
            var bookingsQuery = _context.Bookings.AsQueryable();
            var carsQuery = _context.Cars.AsQueryable();

            // Apply filters
            if (filter.StartDate.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.CreatedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.CreatedAt <= filter.EndDate.Value);

            if (filter.BranchId.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.ReceivingBranchId == filter.BranchId.Value || b.DeliveryBranchId == filter.BranchId.Value);

            if (filter.CategoryId.HasValue)
                carsQuery = carsQuery.Where(c => c.CategoryId == filter.CategoryId.Value);

            var bookings = await bookingsQuery.ToListAsync();
            var cars = await carsQuery.ToListAsync();
            var totalCustomers = await _userManager.Users.CountAsync();

            var totalBookings = bookings.Count;
            var completedBookings = bookings.Count(b => b.Status == BookingStatus.Completed);
            var pendingBookings = bookings.Count(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.InProgress);
            var totalRevenue = bookings.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.FinalAmount);
            var averageBookingValue = completedBookings > 0 ? totalRevenue / completedBookings : 0;

            var activeCars = cars.Count(c => c.Status == CarStatus.Available);
            var rentedCars = cars.Count(c => c.Status == CarStatus.Rented);
            var carUtilizationRate = cars.Count > 0 ? (double)rentedCars / cars.Count * 100 : 0;

            var overallStats = new OverallStatsDto
            {
                TotalBookings = totalBookings,
                TotalRevenue = totalRevenue,
                ActiveCars = activeCars,
                TotalCustomers = totalCustomers,
                PendingBookings = pendingBookings,
                CompletedBookings = completedBookings,
                AverageBookingValue = averageBookingValue,
                CarUtilizationRate = carUtilizationRate
            };

            return ApiResponse<OverallStatsDto>.Success(overallStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting overall stats");
            return ApiResponse<OverallStatsDto>.Error("An error occurred while retrieving overall statistics", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<RevenueStatsDto>> GetRevenueStatsAsync(DashboardFilterDto filter)
    {
        try
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var yearStart = new DateTime(today.Year, 1, 1);
            var previousMonthStart = monthStart.AddMonths(-1);
            var previousMonthEnd = monthStart.AddDays(-1);

            var bookingsQuery = _context.Bookings
                .Where(b => b.Status == BookingStatus.Completed)
                .AsQueryable();

            if (filter.BranchId.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.ReceivingBranchId == filter.BranchId.Value || b.DeliveryBranchId == filter.BranchId.Value);

            var allBookings = await bookingsQuery.ToListAsync();

            var todayRevenue = allBookings.Where(b => b.CreatedAt.Date == today).Sum(b => b.FinalAmount);
            var weekRevenue = allBookings.Where(b => b.CreatedAt.Date >= weekStart).Sum(b => b.FinalAmount);
            var monthRevenue = allBookings.Where(b => b.CreatedAt.Date >= monthStart).Sum(b => b.FinalAmount);
            var yearRevenue = allBookings.Where(b => b.CreatedAt.Date >= yearStart).Sum(b => b.FinalAmount);
            var previousMonthRevenue = allBookings.Where(b => b.CreatedAt.Date >= previousMonthStart && b.CreatedAt.Date <= previousMonthEnd).Sum(b => b.FinalAmount);

            var monthlyGrowthPercentage = previousMonthRevenue > 0 
                ? (double)((monthRevenue - previousMonthRevenue) / previousMonthRevenue) * 100 
                : 0;

            // Generate daily revenue for the last 30 days
            var dailyRevenue = GenerateDailyRevenue(allBookings, today.AddDays(-30), today);

            // Generate monthly revenue for the last 12 months
            var monthlyRevenue = GenerateMonthlyRevenue(allBookings, monthStart.AddMonths(-12), monthStart);

            var revenueStats = new RevenueStatsDto
            {
                TodayRevenue = todayRevenue,
                WeekRevenue = weekRevenue,
                MonthRevenue = monthRevenue,
                YearRevenue = yearRevenue,
                PreviousMonthRevenue = previousMonthRevenue,
                MonthlyGrowthPercentage = monthlyGrowthPercentage,
                DailyRevenue = dailyRevenue,
                MonthlyRevenue = monthlyRevenue
            };

            return ApiResponse<RevenueStatsDto>.Success(revenueStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue stats");
            return ApiResponse<RevenueStatsDto>.Error("An error occurred while retrieving revenue statistics", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<BookingStatsDto>> GetBookingStatsAsync(DashboardFilterDto filter)
    {
        try
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var yearStart = new DateTime(today.Year, 1, 1);

            var bookingsQuery = _context.Bookings.AsQueryable();

            if (filter.BranchId.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.ReceivingBranchId == filter.BranchId.Value || b.DeliveryBranchId == filter.BranchId.Value);

            var allBookings = await bookingsQuery.ToListAsync();

            var todayBookings = allBookings.Count(b => b.CreatedAt.Date == today);
            var weekBookings = allBookings.Count(b => b.CreatedAt.Date >= weekStart);
            var monthBookings = allBookings.Count(b => b.CreatedAt.Date >= monthStart);
            var yearBookings = allBookings.Count(b => b.CreatedAt.Date >= yearStart);
            var cancelledBookings = allBookings.Count(b => b.Status == BookingStatus.Canceled);

            var cancellationRate = allBookings.Count > 0 ? (double)cancelledBookings / allBookings.Count * 100 : 0;

            // Generate booking status breakdown
            var bookingsByStatus = GenerateBookingStatusBreakdown(allBookings);

            // Generate daily bookings for the last 30 days
            var dailyBookings = GenerateDailyBookings(allBookings, today.AddDays(-30), today);

            var bookingStats = new BookingStatsDto
            {
                TodayBookings = todayBookings,
                WeekBookings = weekBookings,
                MonthBookings = monthBookings,
                YearBookings = yearBookings,
                CancelledBookings = cancelledBookings,
                CancellationRate = cancellationRate,
                BookingsByStatus = bookingsByStatus,
                DailyBookings = dailyBookings
            };

            return ApiResponse<BookingStatsDto>.Success(bookingStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting booking stats");
            return ApiResponse<BookingStatsDto>.Error("An error occurred while retrieving booking statistics", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<CarStatsDto>> GetCarStatsAsync(DashboardFilterDto filter)
    {
        try
        {
            var carsQuery = _context.Cars
                .Include(c => c.Category)
                .Include(c => c.Branch)
                .AsQueryable();

            if (filter.BranchId.HasValue)
                carsQuery = carsQuery.Where(c => c.BranchId == filter.BranchId.Value);

            if (filter.CategoryId.HasValue)
                carsQuery = carsQuery.Where(c => c.CategoryId == filter.CategoryId.Value);

            var cars = await carsQuery.ToListAsync();

            var totalCars = cars.Count;
            var availableCars = cars.Count(c => c.Status == CarStatus.Available);
            var rentedCars = cars.Count(c => c.Status == CarStatus.Rented);
            var maintenanceCars = cars.Count(c => c.Status == CarStatus.Maintenance);
            var outOfServiceCars = cars.Count(c => c.Status == CarStatus.OutOfService);

            var utilizationRate = totalCars > 0 ? (double)rentedCars / totalCars * 100 : 0;

            // Generate car statistics by category
            var carsByCategory = await GenerateCarCategoryStats(cars);

            // Generate car statistics by branch
            var carsByBranch = await GenerateCarBranchStats(cars);

            var carStats = new CarStatsDto
            {
                TotalCars = totalCars,
                AvailableCars = availableCars,
                RentedCars = rentedCars,
                MaintenanceCars = maintenanceCars,
                OutOfServiceCars = outOfServiceCars,
                UtilizationRate = utilizationRate,
                CarsByCategory = carsByCategory,
                CarsByBranch = carsByBranch
            };

            return ApiResponse<CarStatsDto>.Success(carStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting car stats");
            return ApiResponse<CarStatsDto>.Error("An error occurred while retrieving car statistics", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<CustomerStatsDto>> GetCustomerStatsAsync(DashboardFilterDto filter)
    {
        try
        {
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var lastMonthStart = monthStart.AddMonths(-1);

            var customers = await _userManager.Users
                .Include(u => u.Bookings)
                .ToListAsync();

            var totalCustomers = customers.Count;
            var newCustomersThisMonth = customers.Count(c => c.CreatedAt >= monthStart);
            var activeCustomers = customers.Count(c => c.Bookings.Any(b => b.Status == BookingStatus.Completed));

            // Calculate customer retention rate (customers who made bookings in both last month and this month)
            var customersLastMonth = customers.Where(c => c.Bookings.Any(b => b.CreatedAt >= lastMonthStart && b.CreatedAt < monthStart)).ToList();
            var customersThisMonth = customers.Where(c => c.Bookings.Any(b => b.CreatedAt >= monthStart)).ToList();
            var retainedCustomers = customersLastMonth.Where(c => customersThisMonth.Contains(c)).Count();
            var customerRetentionRate = customersLastMonth.Count > 0 ? (double)retainedCustomers / customersLastMonth.Count * 100 : 0;

            var averageLoyaltyPoints = customers.Any() ? (decimal)customers.Average(c => c.AvailableLoyaltyPoints) : 0;

            // Generate customer segments
            var customerSegments = GenerateCustomerSegments(customers);

            var customerStats = new CustomerStatsDto
            {
                TotalCustomers = totalCustomers,
                NewCustomersThisMonth = newCustomersThisMonth,
                ActiveCustomers = activeCustomers,
                CustomerRetentionRate = customerRetentionRate,
                AverageLoyaltyPoints = averageLoyaltyPoints,
                CustomerSegments = customerSegments
            };

            return ApiResponse<CustomerStatsDto>.Success(customerStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer stats");
            return ApiResponse<CustomerStatsDto>.Error("An error occurred while retrieving customer statistics", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Real-time Data

    public async Task<ApiResponse<List<RecentBookingDto>>> GetRecentBookingsAsync(int count = 10)
    {
        try
        {
            var recentBookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Car)
                .OrderByDescending(b => b.CreatedAt)
                .Take(count)
                .Select(b => new RecentBookingDto
                {
                    BookingId = b.Id,
                    BookingNumber = b.BookingNumber,
                    CustomerName = b.User.FullName,
                    CarInfo = $"{b.Car.BrandEn} {b.Car.ModelEn} ({b.Car.PlateNumber})",
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    TotalAmount = b.FinalAmount,
                    Status = b.Status.ToString(),
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();

            return ApiResponse<List<RecentBookingDto>>.Success(recentBookings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent bookings");
            return ApiResponse<List<RecentBookingDto>>.Error("An error occurred while retrieving recent bookings", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<PopularCarDto>>> GetPopularCarsAsync(int count = 10, DashboardFilterDto? filter = null)
    {
        try
        {
            var bookingsQuery = _context.Bookings
                .Include(b => b.Car)
                .Where(b => b.Status == BookingStatus.Completed)
                .AsQueryable();

            if (filter?.StartDate.HasValue == true)
                bookingsQuery = bookingsQuery.Where(b => b.CreatedAt >= filter.StartDate.Value);

            if (filter?.EndDate.HasValue == true)
                bookingsQuery = bookingsQuery.Where(b => b.CreatedAt <= filter.EndDate.Value);

            if (filter?.BranchId.HasValue == true)
                bookingsQuery = bookingsQuery.Where(b => b.Car.BranchId == filter.BranchId.Value);

            var popularCars = await bookingsQuery
                .GroupBy(b => new { b.CarId, b.Car.BrandEn, b.Car.ModelEn, b.Car.PlateNumber })
                .Select(g => new PopularCarDto
                {
                    CarId = g.Key.CarId,
                    Brand = g.Key.BrandEn,
                    Model = g.Key.ModelEn,
                    PlateNumber = g.Key.PlateNumber,
                    BookingCount = g.Count(),
                    Revenue = g.Sum(b => b.FinalAmount),
                    UtilizationRate = 0 // Will be calculated separately if needed
                })
                .OrderByDescending(c => c.BookingCount)
                .Take(count)
                .ToListAsync();

            return ApiResponse<List<PopularCarDto>>.Success(popularCars);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting popular cars");
            return ApiResponse<List<PopularCarDto>>.Error("An error occurred while retrieving popular cars", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Analytics Export

    public async Task<ApiResponse<byte[]>> ExportDashboardReportAsync(DashboardFilterDto filter, string format = "pdf")
    {
        try
        {
            var dashboardStats = await GetDashboardStatsAsync(filter);
            if (!dashboardStats.Succeeded)
            {
                return ApiResponse<byte[]>.Error("Failed to get dashboard stats for export", (string?)null, ApiStatusCode.InternalServerError);
            }

            // Placeholder for actual export implementation
            var reportContent = GenerateReportContent(dashboardStats.Data!, format);
            var reportBytes = System.Text.Encoding.UTF8.GetBytes(reportContent);

            return ApiResponse<byte[]>.Success(reportBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting dashboard report");
            return ApiResponse<byte[]>.Error("An error occurred while exporting dashboard report", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<string>> GetDashboardReportUrlAsync(DashboardFilterDto filter, string format = "pdf")
    {
        try
        {
            // Placeholder for actual report URL generation
            var reportId = Guid.NewGuid().ToString();
            var reportUrl = $"/api/admin/dashboard/reports/{reportId}.{format}";

            return ApiResponse<string>.Success(reportUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard report URL");
            return ApiResponse<string>.Error("An error occurred while generating dashboard report URL", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Helper Methods

    private List<DailyRevenueDto> GenerateDailyRevenue(List<Booking> bookings, DateTime startDate, DateTime endDate)
    {
        var dailyRevenue = new List<DailyRevenueDto>();

        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            var dayBookings = bookings.Where(b => b.CreatedAt.Date == date && b.Status == BookingStatus.Completed).ToList();

            dailyRevenue.Add(new DailyRevenueDto
            {
                Date = date,
                Revenue = dayBookings.Sum(b => b.FinalAmount),
                BookingCount = dayBookings.Count
            });
        }

        return dailyRevenue;
    }

    private List<MonthlyRevenueDto> GenerateMonthlyRevenue(List<Booking> bookings, DateTime startDate, DateTime endDate)
    {
        var monthlyRevenue = bookings
            .Where(b => b.CreatedAt >= startDate && b.CreatedAt <= endDate && b.Status == BookingStatus.Completed)
            .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
            .Select(g => new MonthlyRevenueDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM"),
                Revenue = g.Sum(b => b.FinalAmount),
                BookingCount = g.Count()
            })
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month)
            .ToList();

        return monthlyRevenue;
    }

    private List<BookingStatusCountDto> GenerateBookingStatusBreakdown(List<Booking> bookings)
    {
        var totalBookings = bookings.Count;

        var statusBreakdown = bookings
            .GroupBy(b => b.Status)
            .Select(g => new BookingStatusCountDto
            {
                Status = g.Key.ToString(),
                Count = g.Count(),
                Percentage = totalBookings > 0 ? (double)g.Count() / totalBookings * 100 : 0
            })
            .OrderByDescending(s => s.Count)
            .ToList();

        return statusBreakdown;
    }

    private List<DailyBookingDto> GenerateDailyBookings(List<Booking> bookings, DateTime startDate, DateTime endDate)
    {
        var dailyBookings = new List<DailyBookingDto>();

        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            var dayBookings = bookings.Where(b => b.CreatedAt.Date == date).ToList();

            dailyBookings.Add(new DailyBookingDto
            {
                Date = date,
                BookingCount = dayBookings.Count,
                CompletedCount = dayBookings.Count(b => b.Status == BookingStatus.Completed),
                CancelledCount = dayBookings.Count(b => b.Status == BookingStatus.Canceled)
            });
        }

        return dailyBookings;
    }

    private async Task<List<CarCategoryStatsDto>> GenerateCarCategoryStats(List<Car> cars)
    {
        var categoryStats = cars
            .GroupBy(c => new { c.CategoryId, c.Category.NameEn })
            .Select(g => new CarCategoryStatsDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.NameEn ?? "Unknown",
                TotalCars = g.Count(),
                AvailableCars = g.Count(c => c.Status == CarStatus.Available),
                RentedCars = g.Count(c => c.Status == CarStatus.Rented),
                UtilizationRate = g.Count() > 0 ? (double)g.Count(c => c.Status == CarStatus.Rented) / g.Count() * 100 : 0,
                Revenue = 0 // Would need to calculate from bookings
            })
            .OrderByDescending(c => c.TotalCars)
            .ToList();

        return categoryStats;
    }

    private async Task<List<BranchCarStatsDto>> GenerateCarBranchStats(List<Car> cars)
    {
        var branchStats = cars
            .GroupBy(c => new { c.BranchId, c.Branch.NameEn })
            .Select(g => new BranchCarStatsDto
            {
                BranchId = g.Key.BranchId,
                BranchName = g.Key.NameEn ?? "Unknown",
                TotalCars = g.Count(),
                AvailableCars = g.Count(c => c.Status == CarStatus.Available),
                RentedCars = g.Count(c => c.Status == CarStatus.Rented),
                UtilizationRate = g.Count() > 0 ? (double)g.Count(c => c.Status == CarStatus.Rented) / g.Count() * 100 : 0,
                Revenue = 0 // Would need to calculate from bookings
            })
            .OrderByDescending(b => b.TotalCars)
            .ToList();

        return branchStats;
    }

    private List<CustomerSegmentDto> GenerateCustomerSegments(List<ApplicationUser> customers)
    {
        var segments = new List<CustomerSegmentDto>();
        var totalCustomers = customers.Count;

        if (totalCustomers == 0)
            return segments;

        // Segment by booking frequency
        var frequentCustomers = customers.Where(c => c.Bookings.Count(b => b.Status == BookingStatus.Completed) >= 5).ToList();
        var regularCustomers = customers.Where(c => c.Bookings.Count(b => b.Status == BookingStatus.Completed) >= 2 && c.Bookings.Count(b => b.Status == BookingStatus.Completed) < 5).ToList();
        var newCustomers = customers.Where(c => c.Bookings.Count(b => b.Status == BookingStatus.Completed) < 2).ToList();

        segments.Add(new CustomerSegmentDto
        {
            SegmentName = "Frequent Customers",
            CustomerCount = frequentCustomers.Count,
            Percentage = (double)frequentCustomers.Count / totalCustomers * 100,
            AverageSpending = frequentCustomers.Any() ? frequentCustomers.Average(c => c.Bookings.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.FinalAmount)) : 0
        });

        segments.Add(new CustomerSegmentDto
        {
            SegmentName = "Regular Customers",
            CustomerCount = regularCustomers.Count,
            Percentage = (double)regularCustomers.Count / totalCustomers * 100,
            AverageSpending = regularCustomers.Any() ? regularCustomers.Average(c => c.Bookings.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.FinalAmount)) : 0
        });

        segments.Add(new CustomerSegmentDto
        {
            SegmentName = "New Customers",
            CustomerCount = newCustomers.Count,
            Percentage = (double)newCustomers.Count / totalCustomers * 100,
            AverageSpending = newCustomers.Any() ? newCustomers.Average(c => c.Bookings.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.FinalAmount)) : 0
        });

        return segments;
    }

    private string GenerateReportContent(DashboardStatsDto stats, string format)
    {
        var content = $@"
NOL Car Rental - Dashboard Report
Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC

=== OVERALL STATISTICS ===
Total Bookings: {stats.OverallStats.TotalBookings}
Total Revenue: ${stats.OverallStats.TotalRevenue:N2}
Active Cars: {stats.OverallStats.ActiveCars}
Total Customers: {stats.OverallStats.TotalCustomers}
Pending Bookings: {stats.OverallStats.PendingBookings}
Completed Bookings: {stats.OverallStats.CompletedBookings}
Average Booking Value: ${stats.OverallStats.AverageBookingValue:N2}
Car Utilization Rate: {stats.OverallStats.CarUtilizationRate:F2}%

=== REVENUE STATISTICS ===
Today's Revenue: ${stats.RevenueStats.TodayRevenue:N2}
This Week's Revenue: ${stats.RevenueStats.WeekRevenue:N2}
This Month's Revenue: ${stats.RevenueStats.MonthRevenue:N2}
This Year's Revenue: ${stats.RevenueStats.YearRevenue:N2}
Monthly Growth: {stats.RevenueStats.MonthlyGrowthPercentage:F2}%

=== BOOKING STATISTICS ===
Today's Bookings: {stats.BookingStats.TodayBookings}
This Week's Bookings: {stats.BookingStats.WeekBookings}
This Month's Bookings: {stats.BookingStats.MonthBookings}
This Year's Bookings: {stats.BookingStats.YearBookings}
Cancelled Bookings: {stats.BookingStats.CancelledBookings}
Cancellation Rate: {stats.BookingStats.CancellationRate:F2}%

=== CAR STATISTICS ===
Total Cars: {stats.CarStats.TotalCars}
Available Cars: {stats.CarStats.AvailableCars}
Rented Cars: {stats.CarStats.RentedCars}
Maintenance Cars: {stats.CarStats.MaintenanceCars}
Out of Service Cars: {stats.CarStats.OutOfServiceCars}
Utilization Rate: {stats.CarStats.UtilizationRate:F2}%

=== CUSTOMER STATISTICS ===
Total Customers: {stats.CustomerStats.TotalCustomers}
New Customers This Month: {stats.CustomerStats.NewCustomersThisMonth}
Active Customers: {stats.CustomerStats.ActiveCustomers}
Customer Retention Rate: {stats.CustomerStats.CustomerRetentionRate:F2}%
Average Loyalty Points: {stats.CustomerStats.AverageLoyaltyPoints:N0}

=== TOP POPULAR CARS ===
{string.Join("\n", stats.PopularCars.Take(5).Select(c => $"{c.Brand} {c.Model} ({c.PlateNumber}) - {c.BookingCount} bookings, ${c.Revenue:N2} revenue"))}

=== RECENT BOOKINGS ===
{string.Join("\n", stats.RecentBookings.Take(5).Select(b => $"{b.BookingNumber} - {b.CustomerName} - {b.CarInfo} - ${b.TotalAmount:N2} ({b.Status})"))}
";

        return content;
    }

    #endregion
}
