using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NOL.Application.Common.Interfaces;
using NOL.Application.Common.Interfaces.Admin;
using NOL.Application.Common.Responses;
using NOL.Application.DTOs;
using NOL.Application.DTOs.Admin;
using NOL.Domain.Entities;
using NOL.Domain.Enums;
using NOL.Infrastructure.Data;

namespace NOL.Infrastructure.Services;

public class CustomerManagementService : ICustomerManagementService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<CustomerManagementService> _logger;
    private readonly IEmailService _emailService;
    private readonly ILoyaltyPointRepository _loyaltyPointRepository;

    public CustomerManagementService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<CustomerManagementService> logger,
        IEmailService emailService,
        ILoyaltyPointRepository loyaltyPointRepository)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
        _emailService = emailService;
        _loyaltyPointRepository = loyaltyPointRepository;
    }

    #region Customer CRUD Operations

    public async Task<ApiResponse<AdminCustomerDto>> GetCustomerByIdAsync(string customerId)
    {
        try
        {
            var customer = await _context.Users
                .Include(u => u.Bookings.Take(5))
                    .ThenInclude(b => b.Car)
                .Include(u => u.LoyaltyPointTransactions.Take(5))
                .FirstOrDefaultAsync(u => u.Id == customerId && u.UserRole == UserRole.Customer);

            if (customer == null)
            {
                return ApiResponse<AdminCustomerDto>.Error("Customer not found", (string?)null, ApiStatusCode.NotFound);
            }

            var customerDto = await MapToAdminCustomerDto(customer);
            return ApiResponse<AdminCustomerDto>.Success(customerDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer by ID: {CustomerId}", customerId);
            return ApiResponse<AdminCustomerDto>.Error("An error occurred while retrieving customer", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<AdminCustomerDto>>> GetCustomersAsync(CustomerFilterDto filter)
    {
        try
        {
            var query = _context.Users
                .Include(u => u.Bookings.Take(3))
                    .ThenInclude(b => b.Car)
                .Include(u => u.LoyaltyPointTransactions.Take(3))
                .Where(u => u.UserRole == UserRole.Customer)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.Name))
                query = query.Where(u => u.FullName.Contains(filter.Name));

            if (!string.IsNullOrEmpty(filter.Email))
                query = query.Where(u => u.Email!.Contains(filter.Email));

            if (!string.IsNullOrEmpty(filter.Phone))
                query = query.Where(u => u.PhoneNumber!.Contains(filter.Phone));

            if (filter.IsActive.HasValue)
                query = query.Where(u => u.IsActive == filter.IsActive.Value);

            if (filter.EmailVerified.HasValue)
                query = query.Where(u => u.EmailConfirmed == filter.EmailVerified.Value);

            if (filter.PreferredLanguage.HasValue)
                query = query.Where(u => u.PreferredLanguage == filter.PreferredLanguage.Value);

            if (filter.CreatedDateFrom.HasValue)
                query = query.Where(u => u.CreatedAt >= filter.CreatedDateFrom.Value);

            if (filter.CreatedDateTo.HasValue)
                query = query.Where(u => u.CreatedAt <= filter.CreatedDateTo.Value);

            if (filter.LastLoginFrom.HasValue)
                query = query.Where(u => u.LastLoginDate >= filter.LastLoginFrom.Value);

            if (filter.LastLoginTo.HasValue)
                query = query.Where(u => u.LastLoginDate <= filter.LastLoginTo.Value);

            // Apply sorting
            query = filter.SortBy?.ToLower() switch
            {
                "name" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(u => u.FullName) : query.OrderByDescending(u => u.FullName),
                "email" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email),
                "createdat" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(u => u.CreatedAt) : query.OrderByDescending(u => u.CreatedAt),
                "lastlogin" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(u => u.LastLoginDate) : query.OrderByDescending(u => u.LastLoginDate),
                _ => query.OrderByDescending(u => u.CreatedAt)
            };

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var customers = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var customerDtos = new List<AdminCustomerDto>();
            foreach (var customer in customers)
            {
                customerDtos.Add(await MapToAdminCustomerDto(customer));
            }

            return ApiResponse<List<AdminCustomerDto>>.Success(customerDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customers with filter");
            return ApiResponse<List<AdminCustomerDto>>.Error("An error occurred while retrieving customers", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<AdminCustomerDto>> UpdateCustomerAsync(string customerId, UpdateCustomerDto updateCustomerDto, string adminId)
    {
        try
        {
            var customer = await _userManager.FindByIdAsync(customerId);
            if (customer == null || customer.UserRole != UserRole.Customer)
            {
                return ApiResponse<AdminCustomerDto>.Error("Customer not found", (string?)null, ApiStatusCode.NotFound);
            }

            // Update properties if provided
            if (!string.IsNullOrEmpty(updateCustomerDto.FullName))
                customer.FullName = updateCustomerDto.FullName;

            if (!string.IsNullOrEmpty(updateCustomerDto.Email))
            {
                // Check if email is already taken
                var existingUser = await _userManager.FindByEmailAsync(updateCustomerDto.Email);
                if (existingUser != null && existingUser.Id != customerId)
                {
                    return ApiResponse<AdminCustomerDto>.Error("Email already exists", (string?)null, ApiStatusCode.BadRequest);
                }
                customer.Email = updateCustomerDto.Email;
                customer.UserName = updateCustomerDto.Email;
            }

            if (!string.IsNullOrEmpty(updateCustomerDto.PhoneNumber))
                customer.PhoneNumber = updateCustomerDto.PhoneNumber;

            if (updateCustomerDto.PreferredLanguage.HasValue)
                customer.PreferredLanguage = updateCustomerDto.PreferredLanguage.Value;

            if (updateCustomerDto.IsActive.HasValue)
                customer.IsActive = updateCustomerDto.IsActive.Value;

            if (updateCustomerDto.EmailConfirmed.HasValue)
                customer.EmailConfirmed = updateCustomerDto.EmailConfirmed.Value;

            customer.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(customer);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ApiResponse<AdminCustomerDto>.Error($"Failed to update customer: {errors}", (string?)null, ApiStatusCode.BadRequest);
            }

            // Get updated customer with includes
            var updatedCustomer = await _context.Users
                .Include(u => u.Bookings.Take(5))
                    .ThenInclude(b => b.Car)
                .Include(u => u.LoyaltyPointTransactions.Take(5))
                .FirstOrDefaultAsync(u => u.Id == customerId);

            var customerDto = await MapToAdminCustomerDto(updatedCustomer!);
            return ApiResponse<AdminCustomerDto>.Success(customerDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer: {CustomerId}", customerId);
            return ApiResponse<AdminCustomerDto>.Error("An error occurred while updating customer", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> DeleteCustomerAsync(string customerId, string adminId)
    {
        try
        {
            var customer = await _userManager.FindByIdAsync(customerId);
            if (customer == null || customer.UserRole != UserRole.Customer)
            {
                return ApiResponse.Error("Customer not found", (string?)null, ApiStatusCode.NotFound);
            }

            // Check if customer has active bookings
            var hasActiveBookings = await _context.Bookings
                .AnyAsync(b => b.UserId == customerId && 
                              (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.InProgress));

            if (hasActiveBookings)
            {
                return ApiResponse.Error("Cannot delete customer with active bookings", (string?)null, ApiStatusCode.BadRequest);
            }

            // Soft delete by deactivating
            customer.IsActive = false;
            customer.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(customer);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ApiResponse.Error($"Failed to delete customer: {errors}", (string?)null, ApiStatusCode.BadRequest);
            }

            return ApiResponse.Success("Customer deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer: {CustomerId}", customerId);
            return ApiResponse.Error("An error occurred while deleting customer", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Customer Status Management

    public async Task<ApiResponse<AdminCustomerDto>> ActivateCustomerAsync(string customerId, string adminId)
    {
        try
        {
            var customer = await _userManager.FindByIdAsync(customerId);
            if (customer == null || customer.UserRole != UserRole.Customer)
            {
                return ApiResponse<AdminCustomerDto>.Error("Customer not found", (string?)null, ApiStatusCode.NotFound);
            }

            customer.IsActive = true;
            customer.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(customer);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ApiResponse<AdminCustomerDto>.Error($"Failed to activate customer: {errors}", (string?)null, ApiStatusCode.BadRequest);
            }

            // Get updated customer with includes
            var updatedCustomer = await _context.Users
                .Include(u => u.Bookings.Take(5))
                    .ThenInclude(b => b.Car)
                .Include(u => u.LoyaltyPointTransactions.Take(5))
                .FirstOrDefaultAsync(u => u.Id == customerId);

            var customerDto = await MapToAdminCustomerDto(updatedCustomer!);
            return ApiResponse<AdminCustomerDto>.Success(customerDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating customer: {CustomerId}", customerId);
            return ApiResponse<AdminCustomerDto>.Error("An error occurred while activating customer", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<AdminCustomerDto>> DeactivateCustomerAsync(string customerId, string adminId)
    {
        try
        {
            var customer = await _userManager.FindByIdAsync(customerId);
            if (customer == null || customer.UserRole != UserRole.Customer)
            {
                return ApiResponse<AdminCustomerDto>.Error("Customer not found", (string?)null, ApiStatusCode.NotFound);
            }

            // Check if customer has active bookings
            var hasActiveBookings = await _context.Bookings
                .AnyAsync(b => b.UserId == customerId && 
                              (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.InProgress));

            if (hasActiveBookings)
            {
                return ApiResponse<AdminCustomerDto>.Error("Cannot deactivate customer with active bookings", (string?)null, ApiStatusCode.BadRequest);
            }

            customer.IsActive = false;
            customer.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(customer);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ApiResponse<AdminCustomerDto>.Error($"Failed to deactivate customer: {errors}", (string?)null, ApiStatusCode.BadRequest);
            }

            // Get updated customer with includes
            var updatedCustomer = await _context.Users
                .Include(u => u.Bookings.Take(5))
                    .ThenInclude(b => b.Car)
                .Include(u => u.LoyaltyPointTransactions.Take(5))
                .FirstOrDefaultAsync(u => u.Id == customerId);

            var customerDto = await MapToAdminCustomerDto(updatedCustomer!);
            return ApiResponse<AdminCustomerDto>.Success(customerDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating customer: {CustomerId}", customerId);
            return ApiResponse<AdminCustomerDto>.Error("An error occurred while deactivating customer", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Bulk Operations

    public async Task<ApiResponse> BulkUpdateCustomerStatusAsync(List<string> customerIds, bool isActive, string adminId)
    {
        try
        {
            var customers = await _context.Users
                .Where(u => customerIds.Contains(u.Id) && u.UserRole == UserRole.Customer)
                .ToListAsync();

            foreach (var customer in customers)
            {
                customer.IsActive = isActive;
                customer.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return ApiResponse.Success($"Updated {customers.Count} customers");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating customer status");
            return ApiResponse.Error("An error occurred while updating customer status", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> BulkOperationAsync(BulkCustomerOperationDto operationDto, string adminId)
    {
        try
        {
            switch (operationDto.Operation.ToLower())
            {
                case "activate":
                    return await BulkUpdateCustomerStatusAsync(operationDto.CustomerIds, true, adminId);

                case "deactivate":
                    return await BulkUpdateCustomerStatusAsync(operationDto.CustomerIds, false, adminId);

                case "sendnotification":
                    if (operationDto.NotificationData != null)
                    {
                        operationDto.NotificationData.CustomerIds = operationDto.CustomerIds;
                        return await SendBulkNotificationAsync(operationDto.NotificationData, adminId);
                    }
                    return ApiResponse.Error("Notification data is required", (string?)null, ApiStatusCode.BadRequest);

                case "awardpoints":
                    if (operationDto.LoyaltyPointsData != null)
                    {
                        return await BulkAwardLoyaltyPointsAsync(operationDto.CustomerIds, operationDto.LoyaltyPointsData, adminId);
                    }
                    return ApiResponse.Error("Loyalty points data is required", (string?)null, ApiStatusCode.BadRequest);

                default:
                    return ApiResponse.Error("Invalid operation", (string?)null, ApiStatusCode.BadRequest);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing bulk operation: {Operation}", operationDto.Operation);
            return ApiResponse.Error("An error occurred while performing bulk operation", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> BulkAwardLoyaltyPointsAsync(List<string> customerIds, ManageLoyaltyPointsDto loyaltyPointsDto, string adminId)
    {
        try
        {
            var customers = await _context.Users
                .Where(u => customerIds.Contains(u.Id) && u.UserRole == UserRole.Customer)
                .ToListAsync();

            var transactions = new List<LoyaltyPointTransaction>();

            foreach (var customer in customers)
            {
                var transaction = new LoyaltyPointTransaction
                {
                    UserId = customer.Id,
                    Points = loyaltyPointsDto.Points,
                    TransactionType = loyaltyPointsDto.TransactionType,
                    EarnReason = loyaltyPointsDto.EarnReason,
                    Description = loyaltyPointsDto.Description,
                    TransactionDate = DateTime.UtcNow,
                    ExpiryDate = loyaltyPointsDto.ExpiryDate ?? DateTime.UtcNow.AddYears(1),
                    IsExpired = false
                };

                transactions.Add(transaction);

                // Update customer loyalty points
                customer.AvailableLoyaltyPoints += loyaltyPointsDto.Points;
                customer.TotalLoyaltyPoints += loyaltyPointsDto.Points;
                customer.LifetimePointsEarned += loyaltyPointsDto.Points;
                customer.LastPointsEarnedDate = DateTime.UtcNow;
                customer.UpdatedAt = DateTime.UtcNow;
            }

            _context.LoyaltyPointTransactions.AddRange(transactions);
            await _context.SaveChangesAsync();

            return ApiResponse.Success($"Awarded {loyaltyPointsDto.Points} points to {customers.Count} customers");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk awarding loyalty points");
            return ApiResponse.Error("An error occurred while awarding loyalty points", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Customer Analytics

    public async Task<ApiResponse<CustomerAnalyticsDto>> GetCustomerAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.Users
                .Include(u => u.Bookings)
                    .ThenInclude(b => b.Payments)
                .Include(u => u.LoyaltyPointTransactions)
                .Where(u => u.UserRole == UserRole.Customer)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(u => u.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(u => u.CreatedAt <= endDate.Value);

            var customers = await query.ToListAsync();

            var totalCustomers = customers.Count;
            var activeCustomers = customers.Count(c => c.IsActive);
            var thisMonth = DateTime.UtcNow.AddDays(-30);
            var lastMonth = DateTime.UtcNow.AddDays(-60);
            var newCustomersThisMonth = customers.Count(c => c.CreatedAt >= thisMonth);
            var newCustomersLastMonth = customers.Count(c => c.CreatedAt >= lastMonth && c.CreatedAt < thisMonth);

            var analytics = new CustomerAnalyticsDto
            {
                TotalCustomers = totalCustomers,
                ActiveCustomers = activeCustomers,
                NewCustomersThisMonth = newCustomersThisMonth,
                NewCustomersLastMonth = newCustomersLastMonth,
                CustomerGrowthRate = newCustomersLastMonth > 0
                    ? ((double)(newCustomersThisMonth - newCustomersLastMonth) / newCustomersLastMonth) * 100
                    : 0,
                CustomerRetentionRate = CalculateRetentionRate(customers),
                AverageCustomerValue = customers.Any()
                    ? (decimal)customers.Average(c => (double)c.Bookings.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.FinalAmount))
                    : 0,
                AverageLoyaltyPoints = customers.Any() ? (decimal)customers.Average(c => c.AvailableLoyaltyPoints) : 0,
                CustomerSegments = GenerateCustomerSegmentStats(customers),
                MonthlyStats = GenerateMonthlyCustomerStats(customers),
                LanguageStats = GenerateLanguageStats(customers),
                TopCustomers = GenerateTopCustomers(customers, 10)
            };

            return ApiResponse<CustomerAnalyticsDto>.Success(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer analytics");
            return ApiResponse<CustomerAnalyticsDto>.Error("An error occurred while retrieving customer analytics", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<TopCustomerDto>>> GetCustomersStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.Users
                .Include(u => u.Bookings)
                .Where(u => u.UserRole == UserRole.Customer)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(u => u.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(u => u.CreatedAt <= endDate.Value);

            var customers = await query.ToListAsync();

            var stats = customers.Select(customer => new TopCustomerDto
            {
                CustomerId = customer.Id,
                CustomerName = customer.FullName,
                Email = customer.Email ?? "",
                TotalBookings = customer.Bookings.Count,
                TotalSpent = customer.Bookings.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.FinalAmount),
                LoyaltyPoints = customer.AvailableLoyaltyPoints,
                LastBookingDate = customer.Bookings.OrderByDescending(b => b.CreatedAt).FirstOrDefault()?.CreatedAt ?? DateTime.MinValue,
                CustomerSegment = DetermineCustomerSegment(customer)
            }).ToList();

            return ApiResponse<List<TopCustomerDto>>.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customers stats");
            return ApiResponse<List<TopCustomerDto>>.Error("An error occurred while retrieving customers stats", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<TopCustomerDto>>> GetTopCustomersAsync(int count = 10, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.Users
                .Include(u => u.Bookings)
                .Where(u => u.UserRole == UserRole.Customer)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(u => u.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(u => u.CreatedAt <= endDate.Value);

            var customers = await query.ToListAsync();
            var topCustomers = GenerateTopCustomers(customers, count);

            return ApiResponse<List<TopCustomerDto>>.Success(topCustomers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top customers");
            return ApiResponse<List<TopCustomerDto>>.Error("An error occurred while retrieving top customers", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<CustomerSegmentStatsDto>>> GetCustomerSegmentationAsync()
    {
        try
        {
            var customers = await _context.Users
                .Include(u => u.Bookings)
                .Where(u => u.UserRole == UserRole.Customer)
                .ToListAsync();

            var segmentStats = GenerateCustomerSegmentStats(customers);
            return ApiResponse<List<CustomerSegmentStatsDto>>.Success(segmentStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer segmentation");
            return ApiResponse<List<CustomerSegmentStatsDto>>.Error("An error occurred while retrieving customer segmentation", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<CustomerBookingSummaryDto>>> GetCustomerBookingHistoryAsync(string customerId, int page = 1, int pageSize = 10)
    {
        try
        {
            var customer = await _userManager.FindByIdAsync(customerId);
            if (customer == null || customer.UserRole != UserRole.Customer)
            {
                return ApiResponse<List<CustomerBookingSummaryDto>>.Error("Customer not found", (string?)null, ApiStatusCode.NotFound);
            }

            var bookings = await _context.Bookings
                .Include(b => b.Car)
                .Where(b => b.UserId == customerId)
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var bookingSummaries = bookings.Select(b => new CustomerBookingSummaryDto
            {
                BookingId = b.Id,
                BookingNumber = b.BookingNumber,
                CarInfo = $"{b.Car?.BrandEn} {b.Car?.ModelEn}",
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                TotalAmount = b.FinalAmount,
                Status = b.Status,
                CreatedAt = b.CreatedAt
            }).ToList();

            return ApiResponse<List<CustomerBookingSummaryDto>>.Success(bookingSummaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer booking history: {CustomerId}", customerId);
            return ApiResponse<List<CustomerBookingSummaryDto>>.Error("An error occurred while retrieving customer booking history", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<decimal>> GetCustomerLifetimeValueAsync(string customerId)
    {
        try
        {
            var customer = await _userManager.FindByIdAsync(customerId);
            if (customer == null || customer.UserRole != UserRole.Customer)
            {
                return ApiResponse<decimal>.Error("Customer not found", (string?)null, ApiStatusCode.NotFound);
            }

            var lifetimeValue = await _context.Bookings
                .Where(b => b.UserId == customerId && b.Status == BookingStatus.Completed)
                .SumAsync(b => b.FinalAmount);

            return ApiResponse<decimal>.Success(lifetimeValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer lifetime value: {CustomerId}", customerId);
            return ApiResponse<decimal>.Error("An error occurred while retrieving customer lifetime value", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<double>> GetCustomerSatisfactionRatingAsync(string customerId)
    {
        try
        {
            var customer = await _userManager.FindByIdAsync(customerId);
            if (customer == null || customer.UserRole != UserRole.Customer)
            {
                return ApiResponse<double>.Error("Customer not found", (string?)null, ApiStatusCode.NotFound);
            }

            var averageRating = await _context.Reviews
                .Where(r => r.UserId == customerId)
                .AverageAsync(r => (double?)r.Rating) ?? 0.0;

            return ApiResponse<double>.Success(averageRating);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer satisfaction rating: {CustomerId}", customerId);
            return ApiResponse<double>.Error("An error occurred while retrieving customer satisfaction rating", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Loyalty Points Management

    public async Task<ApiResponse<CustomerLoyaltyTransactionDto>> AwardLoyaltyPointsAsync(ManageLoyaltyPointsDto loyaltyPointsDto, string adminId)
    {
        try
        {
            var customer = await _userManager.FindByIdAsync(loyaltyPointsDto.UserId);
            if (customer == null || customer.UserRole != UserRole.Customer)
            {
                return ApiResponse<CustomerLoyaltyTransactionDto>.Error("Customer not found", (string?)null, ApiStatusCode.NotFound);
            }

            var transaction = new LoyaltyPointTransaction
            {
                UserId = loyaltyPointsDto.UserId,
                Points = loyaltyPointsDto.Points,
                TransactionType = loyaltyPointsDto.TransactionType,
                EarnReason = loyaltyPointsDto.EarnReason,
                Description = loyaltyPointsDto.Description,
                TransactionDate = DateTime.UtcNow,
                ExpiryDate = loyaltyPointsDto.ExpiryDate ?? DateTime.UtcNow.AddYears(1),
                IsExpired = false
            };

            var createdTransaction = await _loyaltyPointRepository.CreateTransactionAsync(transaction);

            // Update customer loyalty points
            customer.AvailableLoyaltyPoints += loyaltyPointsDto.Points;
            customer.TotalLoyaltyPoints += loyaltyPointsDto.Points;
            customer.LifetimePointsEarned += loyaltyPointsDto.Points;
            customer.LastPointsEarnedDate = DateTime.UtcNow;
            customer.UpdatedAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(customer);

            var transactionDto = new CustomerLoyaltyTransactionDto
            {
                Id = createdTransaction.Id,
                Points = createdTransaction.Points,
                TransactionType = createdTransaction.TransactionType,
                EarnReason = createdTransaction.EarnReason,
                Description = createdTransaction.Description,
                TransactionDate = createdTransaction.TransactionDate,
                ExpiryDate = createdTransaction.ExpiryDate,
                IsExpired = createdTransaction.IsExpired
            };

            return ApiResponse<CustomerLoyaltyTransactionDto>.Success(transactionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error awarding loyalty points to customer: {CustomerId}", loyaltyPointsDto.UserId);
            return ApiResponse<CustomerLoyaltyTransactionDto>.Error("An error occurred while awarding loyalty points", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<CustomerLoyaltyTransactionDto>> DeductLoyaltyPointsAsync(ManageLoyaltyPointsDto loyaltyPointsDto, string adminId)
    {
        try
        {
            var customer = await _userManager.FindByIdAsync(loyaltyPointsDto.UserId);
            if (customer == null || customer.UserRole != UserRole.Customer)
            {
                return ApiResponse<CustomerLoyaltyTransactionDto>.Error("Customer not found", (string?)null, ApiStatusCode.NotFound);
            }

            if (customer.AvailableLoyaltyPoints < loyaltyPointsDto.Points)
            {
                return ApiResponse<CustomerLoyaltyTransactionDto>.Error("Insufficient loyalty points", (string?)null, ApiStatusCode.BadRequest);
            }

            var transaction = new LoyaltyPointTransaction
            {
                UserId = loyaltyPointsDto.UserId,
                Points = -loyaltyPointsDto.Points, // Negative for deduction
                TransactionType = LoyaltyPointTransactionType.Redeemed,
                Description = loyaltyPointsDto.Description,
                TransactionDate = DateTime.UtcNow,
                IsExpired = false
            };

            var createdTransaction = await _loyaltyPointRepository.CreateTransactionAsync(transaction);

            // Update customer loyalty points
            customer.AvailableLoyaltyPoints -= loyaltyPointsDto.Points;
            customer.LifetimePointsRedeemed += loyaltyPointsDto.Points;
            customer.UpdatedAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(customer);

            var transactionDto = new CustomerLoyaltyTransactionDto
            {
                Id = createdTransaction.Id,
                Points = createdTransaction.Points,
                TransactionType = createdTransaction.TransactionType,
                Description = createdTransaction.Description,
                TransactionDate = createdTransaction.TransactionDate,
                IsExpired = createdTransaction.IsExpired
            };

            return ApiResponse<CustomerLoyaltyTransactionDto>.Success(transactionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deducting loyalty points from customer: {CustomerId}", loyaltyPointsDto.UserId);
            return ApiResponse<CustomerLoyaltyTransactionDto>.Error("An error occurred while deducting loyalty points", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<CustomerLoyaltyTransactionDto>>> GetCustomerLoyaltyTransactionsAsync(string customerId, int page = 1, int pageSize = 10)
    {
        try
        {
            var customer = await _userManager.FindByIdAsync(customerId);
            if (customer == null || customer.UserRole != UserRole.Customer)
            {
                return ApiResponse<List<CustomerLoyaltyTransactionDto>>.Error("Customer not found", (string?)null, ApiStatusCode.NotFound);
            }

            var transactions = await _loyaltyPointRepository.GetUserTransactionsAsync(customerId, page, pageSize);

            var transactionDtos = transactions.Select(t => new CustomerLoyaltyTransactionDto
            {
                Id = t.Id,
                Points = t.Points,
                TransactionType = t.TransactionType,
                EarnReason = t.EarnReason,
                Description = t.Description,
                TransactionDate = t.TransactionDate,
                ExpiryDate = t.ExpiryDate,
                IsExpired = t.IsExpired
            }).ToList();

            return ApiResponse<List<CustomerLoyaltyTransactionDto>>.Success(transactionDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer loyalty points history: {CustomerId}", customerId);
            return ApiResponse<List<CustomerLoyaltyTransactionDto>>.Error("An error occurred while retrieving loyalty points history", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<int>> GetCustomerAvailablePointsAsync(string customerId)
    {
        try
        {
            var customer = await _userManager.FindByIdAsync(customerId);
            if (customer == null || customer.UserRole != UserRole.Customer)
            {
                return ApiResponse<int>.Error("Customer not found", (string?)null, ApiStatusCode.NotFound);
            }

            return ApiResponse<int>.Success(customer.AvailableLoyaltyPoints);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer available points: {CustomerId}", customerId);
            return ApiResponse<int>.Error("An error occurred while retrieving customer available points", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> ExpireLoyaltyPointsAsync(string customerId, string adminId)
    {
        try
        {
            var customer = await _userManager.FindByIdAsync(customerId);
            if (customer == null || customer.UserRole != UserRole.Customer)
            {
                return ApiResponse.Error("Customer not found", (string?)null, ApiStatusCode.NotFound);
            }

            var expiredTransactions = await _context.LoyaltyPointTransactions
                .Where(t => t.UserId == customerId &&
                           t.ExpiryDate.HasValue &&
                           t.ExpiryDate.Value <= DateTime.UtcNow &&
                           !t.IsExpired)
                .ToListAsync();

            var expiredPoints = expiredTransactions.Sum(t => t.Points);

            foreach (var transaction in expiredTransactions)
            {
                transaction.IsExpired = true;
            }

            // Update customer available points
            customer.AvailableLoyaltyPoints = Math.Max(0, customer.AvailableLoyaltyPoints - expiredPoints);
            customer.UpdatedAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(customer);
            await _context.SaveChangesAsync();

            return ApiResponse.Success($"Expired {expiredPoints} points for customer");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error expiring loyalty points for customer: {CustomerId}", customerId);
            return ApiResponse.Error("An error occurred while expiring loyalty points", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Customer Communication

    public async Task<ApiResponse> SendNotificationToCustomerAsync(string customerId, SendCustomerNotificationDto notificationDto, string adminId)
    {
        try
        {
            var customer = await _userManager.FindByIdAsync(customerId);
            if (customer == null || customer.UserRole != UserRole.Customer)
            {
                return ApiResponse.Error("Customer not found", (string?)null, ApiStatusCode.NotFound);
            }

            var notification = new Notification
            {
                UserId = customerId,
                TitleEn = notificationDto.TitleEn,
                TitleAr = notificationDto.TitleAr,
                MessageEn = notificationDto.MessageEn,
                MessageAr = notificationDto.MessageAr,
                Type = notificationDto.Type,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send email if requested
            if (notificationDto.SendEmail && !string.IsNullOrEmpty(customer.Email))
            {
                var title = customer.PreferredLanguage == Language.Arabic ? notificationDto.TitleAr : notificationDto.TitleEn;
                var message = customer.PreferredLanguage == Language.Arabic ? notificationDto.MessageAr : notificationDto.MessageEn;

                // Use a generic email method (you might want to create a specific one for notifications)
                await _emailService.SendWelcomeEmailAsync(customer.Email, customer.FullName);
            }

            return ApiResponse.Success("Notification sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to customer: {CustomerId}", customerId);
            return ApiResponse.Error("An error occurred while sending notification", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> SendBulkNotificationAsync(SendCustomerNotificationDto notificationDto, string adminId)
    {
        try
        {
            var customers = await _context.Users
                .Where(u => u.UserRole == UserRole.Customer && u.IsActive)
                .ToListAsync();

            var notifications = new List<Notification>();
            foreach (var customer in customers)
            {
                notifications.Add(new Notification
                {
                    UserId = customer.Id,
                    TitleEn = notificationDto.TitleEn,
                    TitleAr = notificationDto.TitleAr,
                    MessageEn = notificationDto.MessageEn,
                    MessageAr = notificationDto.MessageAr,
                    Type = notificationDto.Type,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                });
            }

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            // Send emails if requested
            if (notificationDto.SendEmail)
            {
                foreach (var customer in customers.Where(c => !string.IsNullOrEmpty(c.Email)))
                {
                    try
                    {
                        await _emailService.SendWelcomeEmailAsync(customer.Email!, customer.FullName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to send email to customer: {CustomerId}", customer.Id);
                    }
                }
            }

            return ApiResponse.Success($"Bulk notification sent to {customers.Count} customers");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk notification");
            return ApiResponse.Error("An error occurred while sending bulk notification", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> SendWelcomeEmailAsync(string customerId, string adminId)
    {
        try
        {
            var customer = await _userManager.FindByIdAsync(customerId);
            if (customer == null || customer.UserRole != UserRole.Customer)
            {
                return ApiResponse.Error("Customer not found", (string?)null, ApiStatusCode.NotFound);
            }

            if (string.IsNullOrEmpty(customer.Email))
            {
                return ApiResponse.Error("Customer email not found", (string?)null, ApiStatusCode.BadRequest);
            }

            var emailSent = await _emailService.SendWelcomeEmailAsync(customer.Email, customer.FullName);
            if (!emailSent)
            {
                return ApiResponse.Error("Failed to send welcome email", (string?)null, ApiStatusCode.InternalServerError);
            }

            return ApiResponse.Success("Welcome email sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending welcome email to customer: {CustomerId}", customerId);
            return ApiResponse.Error("An error occurred while sending welcome email", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> SendCustomerReportAsync(string customerId, string adminId)
    {
        try
        {
            var customer = await _userManager.FindByIdAsync(customerId);
            if (customer == null || customer.UserRole != UserRole.Customer)
            {
                return ApiResponse.Error("Customer not found", (string?)null, ApiStatusCode.NotFound);
            }

            if (string.IsNullOrEmpty(customer.Email))
            {
                return ApiResponse.Error("Customer email not found", (string?)null, ApiStatusCode.BadRequest);
            }

            // Get customer analytics for the report
            var analyticsResponse = await GetCustomerAnalyticsAsync();
            if (!analyticsResponse.Succeeded)
            {
                return ApiResponse.Error("Failed to generate customer report", (string?)null, ApiStatusCode.InternalServerError);
            }

            // For now, send a generic email. In a real implementation, you would generate a PDF report
            var emailSent = await _emailService.SendWelcomeEmailAsync(customer.Email, customer.FullName);
            if (!emailSent)
            {
                return ApiResponse.Error("Failed to send customer report", (string?)null, ApiStatusCode.InternalServerError);
            }

            return ApiResponse.Success("Customer report sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending customer report: {CustomerId}", customerId);
            return ApiResponse.Error("An error occurred while sending customer report", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Customer Segmentation

    public async Task<ApiResponse<string>> GetCustomerSegmentAsync(string customerId)
    {
        try
        {
            var customer = await _context.Users
                .Include(u => u.Bookings)
                .FirstOrDefaultAsync(u => u.Id == customerId && u.UserRole == UserRole.Customer);

            if (customer == null)
            {
                return ApiResponse<string>.Error("Customer not found", (string?)null, ApiStatusCode.NotFound);
            }

            var segment = DetermineCustomerSegment(customer);
            return ApiResponse<string>.Success(segment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer segment: {CustomerId}", customerId);
            return ApiResponse<string>.Error("An error occurred while determining customer segment", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<AdminCustomerDto>>> GetCustomersBySegmentAsync(string segment, int page = 1, int pageSize = 10)
    {
        try
        {
            var customers = await _context.Users
                .Include(u => u.Bookings.Take(3))
                    .ThenInclude(b => b.Car)
                .Include(u => u.LoyaltyPointTransactions.Take(3))
                .Where(u => u.UserRole == UserRole.Customer)
                .ToListAsync();

            // Filter customers by segment
            var filteredCustomers = customers.Where(c => DetermineCustomerSegment(c) == segment).ToList();

            // Apply pagination
            var paginatedCustomers = filteredCustomers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var customerDtos = new List<AdminCustomerDto>();
            foreach (var customer in paginatedCustomers)
            {
                customerDtos.Add(await MapToAdminCustomerDto(customer));
            }

            return ApiResponse<List<AdminCustomerDto>>.Success(customerDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customers by segment: {Segment}", segment);
            return ApiResponse<List<AdminCustomerDto>>.Error("An error occurred while retrieving customers by segment", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<string>>> GetAvailableCustomerSegmentsAsync()
    {
        try
        {
            var segments = new List<string>
            {
                "New Customer",
                "Regular Customer",
                "VIP Customer",
                "Inactive Customer",
                "High Value Customer"
            };

            return ApiResponse<List<string>>.Success(segments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available customer segments");
            return ApiResponse<List<string>>.Error("An error occurred while retrieving customer segments", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Customer Reports

    public async Task<ApiResponse<CustomerReportDto>> GenerateCustomerReportAsync(CustomerFilterDto filter)
    {
        try
        {
            var customersResponse = await GetCustomersAsync(filter);
            if (!customersResponse.Succeeded)
            {
                return ApiResponse<CustomerReportDto>.Error("Failed to generate customer report", (string?)null, ApiStatusCode.InternalServerError);
            }

            var analyticsResponse = await GetCustomerAnalyticsAsync(filter.CreatedDateFrom, filter.CreatedDateTo);
            if (!analyticsResponse.Succeeded)
            {
                return ApiResponse<CustomerReportDto>.Error("Failed to generate customer analytics", (string?)null, ApiStatusCode.InternalServerError);
            }

            var customers = customersResponse.Data ?? new List<AdminCustomerDto>();
            var analytics = analyticsResponse.Data ?? new CustomerAnalyticsDto();

            var report = new CustomerReportDto
            {
                GeneratedAt = DateTime.UtcNow,
                StartDate = filter.CreatedDateFrom,
                EndDate = filter.CreatedDateTo,
                Analytics = analytics,
                Customers = customers,
                Summary = new CustomerReportSummaryDto
                {
                    TotalCustomers = customers.Count,
                    ActiveCustomers = customers.Count(c => c.UserRole == UserRole.Customer), // Placeholder since AdminCustomerDto doesn't have IsActive
                    InactiveCustomers = 0, // Placeholder
                    NewCustomers = customers.Count(c => filter.CreatedDateFrom.HasValue && c.CreatedAt >= filter.CreatedDateFrom.Value),
                    TotalCustomerValue = customers.Sum(c => c.TotalSpent),
                    AverageCustomerValue = customers.Any() ? customers.Average(c => c.TotalSpent) : 0,
                    TotalBookings = customers.Sum(c => c.TotalBookings),
                    TotalRevenue = customers.Sum(c => c.TotalSpent),
                    TotalLoyaltyPointsIssued = customers.Sum(c => c.LifetimePointsEarned),
                    TotalLoyaltyPointsRedeemed = customers.Sum(c => c.LifetimePointsRedeemed),
                    CustomerRetentionRate = analytics.CustomerRetentionRate,
                    CustomerSatisfactionRate = 85.0 // Placeholder - would calculate from reviews
                }
            };

            return ApiResponse<CustomerReportDto>.Success(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating customer report");
            return ApiResponse<CustomerReportDto>.Error("An error occurred while generating customer report", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<byte[]>> ExportCustomerReportAsync(CustomerFilterDto filter, string format = "excel")
    {
        try
        {
            var reportResponse = await GenerateCustomerReportAsync(filter);
            if (!reportResponse.Succeeded)
            {
                return ApiResponse<byte[]>.Error("Failed to generate report for export", (string?)null, ApiStatusCode.InternalServerError);
            }

            // Placeholder for actual export implementation
            // In a real implementation, you would use libraries like EPPlus for Excel or iTextSharp for PDF
            var dummyData = System.Text.Encoding.UTF8.GetBytes("Customer Report Export - Implementation needed");

            return ApiResponse<byte[]>.Success(dummyData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting customer report");
            return ApiResponse<byte[]>.Error("An error occurred while exporting customer report", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<byte[]>> ExportCustomerLoyaltyReportAsync(DateTime? startDate = null, DateTime? endDate = null, string format = "excel")
    {
        try
        {
            var query = _context.LoyaltyPointTransactions
                .Include(t => t.User)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(t => t.TransactionDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(t => t.TransactionDate <= endDate.Value);

            var transactions = await query.ToListAsync();

            // Placeholder for actual export implementation
            var dummyData = System.Text.Encoding.UTF8.GetBytes($"Loyalty Report Export - {transactions.Count} transactions");

            return ApiResponse<byte[]>.Success(dummyData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting customer loyalty report");
            return ApiResponse<byte[]>.Error("An error occurred while exporting loyalty report", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Customer Retention Analysis

    public async Task<ApiResponse<double>> GetCustomerRetentionRateAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.Users
                .Include(u => u.Bookings)
                .Where(u => u.UserRole == UserRole.Customer)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(u => u.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(u => u.CreatedAt <= endDate.Value);

            var customers = await query.ToListAsync();
            var retentionRate = CalculateRetentionRate(customers);

            return ApiResponse<double>.Success(retentionRate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer retention rate");
            return ApiResponse<double>.Error("An error occurred while calculating retention rate", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<AdminCustomerDto>>> GetChurnRiskCustomersAsync(int count = 50)
    {
        try
        {
            var customers = await _context.Users
                .Include(u => u.Bookings.Take(3))
                    .ThenInclude(b => b.Car)
                .Include(u => u.LoyaltyPointTransactions.Take(3))
                .Where(u => u.UserRole == UserRole.Customer && u.IsActive)
                .ToListAsync();

            // Identify customers at risk of churning (no bookings in last 90 days, declining activity, etc.)
            var churnRiskCustomers = customers
                .Where(c =>
                {
                    var lastBooking = c.Bookings?.OrderByDescending(b => b.CreatedAt).FirstOrDefault();
                    return lastBooking == null || (DateTime.UtcNow - lastBooking.CreatedAt).Days > 90;
                })
                .Take(count)
                .ToList();

            var customerDtos = new List<AdminCustomerDto>();
            foreach (var customer in churnRiskCustomers)
            {
                customerDtos.Add(await MapToAdminCustomerDto(customer));
            }

            return ApiResponse<List<AdminCustomerDto>>.Success(customerDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting churn risk customers");
            return ApiResponse<List<AdminCustomerDto>>.Error("An error occurred while retrieving churn risk customers", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<AdminCustomerDto>>> GetInactiveCustomersAsync(int daysSinceLastActivity = 90, int page = 1, int pageSize = 10)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysSinceLastActivity);

            var customers = await _context.Users
                .Include(u => u.Bookings.Take(3))
                    .ThenInclude(b => b.Car)
                .Include(u => u.LoyaltyPointTransactions.Take(3))
                .Where(u => u.UserRole == UserRole.Customer &&
                           (u.LastLoginDate == null || u.LastLoginDate < cutoffDate))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var customerDtos = new List<AdminCustomerDto>();
            foreach (var customer in customers)
            {
                customerDtos.Add(await MapToAdminCustomerDto(customer));
            }

            return ApiResponse<List<AdminCustomerDto>>.Success(customerDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inactive customers");
            return ApiResponse<List<AdminCustomerDto>>.Error("An error occurred while retrieving inactive customers", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Customer Validation

    public async Task<ApiResponse<bool>> ValidateCustomerEmailAsync(string email, string? excludeCustomerId = null)
    {
        try
        {
            var query = _context.Users
                .Where(u => u.Email == email && u.UserRole == UserRole.Customer)
                .AsQueryable();

            if (!string.IsNullOrEmpty(excludeCustomerId))
                query = query.Where(u => u.Id != excludeCustomerId);

            var exists = await query.AnyAsync();
            return ApiResponse<bool>.Success(!exists); // Return true if email is available (not exists)
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating customer email: {Email}", email);
            return ApiResponse<bool>.Error("An error occurred while validating email", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<bool>> ValidateCustomerPhoneAsync(string phone, string? excludeCustomerId = null)
    {
        try
        {
            var query = _context.Users
                .Where(u => u.PhoneNumber == phone && u.UserRole == UserRole.Customer)
                .AsQueryable();

            if (!string.IsNullOrEmpty(excludeCustomerId))
                query = query.Where(u => u.Id != excludeCustomerId);

            var exists = await query.AnyAsync();
            return ApiResponse<bool>.Success(!exists); // Return true if phone is available (not exists)
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating customer phone: {Phone}", phone);
            return ApiResponse<bool>.Error("An error occurred while validating phone", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Customer Search

    public async Task<ApiResponse<List<AdminCustomerDto>>> SearchCustomersAsync(string searchTerm, int page = 1, int pageSize = 10)
    {
        try
        {
            var customers = await _context.Users
                .Include(u => u.Bookings.Take(3))
                    .ThenInclude(b => b.Car)
                .Include(u => u.LoyaltyPointTransactions.Take(3))
                .Where(u => u.UserRole == UserRole.Customer &&
                           (u.FullName.Contains(searchTerm) ||
                            u.Email!.Contains(searchTerm) ||
                            u.PhoneNumber!.Contains(searchTerm)))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var customerDtos = new List<AdminCustomerDto>();
            foreach (var customer in customers)
            {
                customerDtos.Add(await MapToAdminCustomerDto(customer));
            }

            return ApiResponse<List<AdminCustomerDto>>.Success(customerDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching customers: {SearchTerm}", searchTerm);
            return ApiResponse<List<AdminCustomerDto>>.Error("An error occurred while searching customers", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<AdminCustomerDto>> GetCustomerByEmailAsync(string email)
    {
        try
        {
            var customer = await _context.Users
                .Include(u => u.Bookings.Take(5))
                    .ThenInclude(b => b.Car)
                .Include(u => u.LoyaltyPointTransactions.Take(5))
                .FirstOrDefaultAsync(u => u.Email == email && u.UserRole == UserRole.Customer);

            if (customer == null)
            {
                return ApiResponse<AdminCustomerDto>.Error("Customer not found", (string?)null, ApiStatusCode.NotFound);
            }

            var customerDto = await MapToAdminCustomerDto(customer);
            return ApiResponse<AdminCustomerDto>.Success(customerDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer by email: {Email}", email);
            return ApiResponse<AdminCustomerDto>.Error("An error occurred while retrieving customer", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<AdminCustomerDto>> GetCustomerByPhoneAsync(string phone)
    {
        try
        {
            var customer = await _context.Users
                .Include(u => u.Bookings.Take(5))
                    .ThenInclude(b => b.Car)
                .Include(u => u.LoyaltyPointTransactions.Take(5))
                .FirstOrDefaultAsync(u => u.PhoneNumber == phone && u.UserRole == UserRole.Customer);

            if (customer == null)
            {
                return ApiResponse<AdminCustomerDto>.Error("Customer not found", (string?)null, ApiStatusCode.NotFound);
            }

            var customerDto = await MapToAdminCustomerDto(customer);
            return ApiResponse<AdminCustomerDto>.Success(customerDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer by phone: {Phone}", phone);
            return ApiResponse<AdminCustomerDto>.Error("An error occurred while retrieving customer", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Helper Methods

    private async Task<AdminCustomerDto> MapToAdminCustomerDto(ApplicationUser customer)
    {
        var recentBookings = customer.Bookings?.Take(5).Select(b => new CustomerBookingSummaryDto
        {
            BookingId = b.Id,
            BookingNumber = b.BookingNumber,
            CarInfo = $"{b.Car?.BrandEn} {b.Car?.ModelEn}",
            StartDate = b.StartDate,
            EndDate = b.EndDate,
            TotalAmount = b.FinalAmount,
            Status = b.Status,
            CreatedAt = b.CreatedAt
        }).ToList() ?? new List<CustomerBookingSummaryDto>();

        var recentTransactions = customer.LoyaltyPointTransactions?.Take(5).Select(t => new CustomerLoyaltyTransactionDto
        {
            Id = t.Id,
            Points = t.Points,
            TransactionType = t.TransactionType,
            EarnReason = t.EarnReason,
            Description = t.Description,
            TransactionDate = t.TransactionDate,
            ExpiryDate = t.ExpiryDate,
            IsExpired = t.IsExpired
        }).ToList() ?? new List<CustomerLoyaltyTransactionDto>();

        return new AdminCustomerDto
        {
            Id = customer.Id,
            FullName = customer.FullName,
            Email = customer.Email ?? "",
            PhoneNumber = customer.PhoneNumber ?? "",
            UserRole = customer.UserRole,
            PreferredLanguage = customer.PreferredLanguage,
            loyaltyPoints = customer.AvailableLoyaltyPoints,
            emailVerified = customer.EmailConfirmed,
            TotalLoyaltyPoints = customer.TotalLoyaltyPoints,
            AvailableLoyaltyPoints = customer.AvailableLoyaltyPoints,
            LifetimePointsEarned = customer.LifetimePointsEarned,
            LifetimePointsRedeemed = customer.LifetimePointsRedeemed,
            LastPointsEarnedDate = customer.LastPointsEarnedDate,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt,
            LastLoginDate = customer.LastLoginDate,
            TotalBookings = customer.Bookings?.Count ?? 0,
            CompletedBookings = customer.Bookings?.Count(b => b.Status == BookingStatus.Completed) ?? 0,
            CancelledBookings = customer.Bookings?.Count(b => b.Status == BookingStatus.Canceled) ?? 0,
            TotalSpent = customer.Bookings?.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.FinalAmount) ?? 0,
            LastBookingDate = customer.Bookings?.OrderByDescending(b => b.CreatedAt).FirstOrDefault()?.CreatedAt,
            CustomerSegment = DetermineCustomerSegment(customer),
            RecentBookings = recentBookings,
            RecentLoyaltyTransactions = recentTransactions
        };
    }

    private string DetermineCustomerSegment(ApplicationUser customer)
    {
        var totalBookings = customer.Bookings?.Count ?? 0;
        var completedBookings = customer.Bookings?.Count(b => b.Status == BookingStatus.Completed) ?? 0;
        var totalSpent = customer.Bookings?.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.FinalAmount) ?? 0;
        var daysSinceLastBooking = customer.Bookings?.Any() == true
            ? (DateTime.UtcNow - customer.Bookings.OrderByDescending(b => b.CreatedAt).First().CreatedAt).Days
            : int.MaxValue;

        // Determine segment based on business rules
        if (totalBookings == 0)
            return "New Customer";

        if (daysSinceLastBooking > 365)
            return "Inactive Customer";

        if (totalSpent > 5000 || completedBookings > 10)
            return "VIP Customer";

        if (totalSpent > 2000 || completedBookings > 5)
            return "High Value Customer";

        if (completedBookings > 1)
            return "Regular Customer";

        return "New Customer";
    }

    private double CalculateRetentionRate(List<ApplicationUser> customers)
    {
        if (!customers.Any()) return 0;

        var activeCustomers = customers.Count(c => c.IsActive);
        var customersWithRecentActivity = customers.Count(c =>
            c.LastLoginDate.HasValue &&
            (DateTime.UtcNow - c.LastLoginDate.Value).Days <= 90);

        return activeCustomers > 0 ? (double)customersWithRecentActivity / activeCustomers * 100 : 0;
    }

    private List<CustomerSegmentStatsDto> GenerateCustomerSegmentStats(List<ApplicationUser> customers)
    {
        var segments = customers.GroupBy(c => DetermineCustomerSegment(c))
            .Select(g => new CustomerSegmentStatsDto
            {
                SegmentName = g.Key,
                CustomerCount = g.Count(),
                Percentage = customers.Any() ? (double)g.Count() / customers.Count * 100 : 0,
                AverageSpending = g.Any() ? g.Average(c => c.Bookings?.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.FinalAmount) ?? 0) : 0,
                TotalRevenue = g.Sum(c => c.Bookings?.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.FinalAmount) ?? 0),
                AverageBookingFrequency = g.Any() ? g.Average(c => c.Bookings?.Count ?? 0) : 0
            })
            .ToList();

        return segments;
    }

    private List<MonthlyCustomerStatsDto> GenerateMonthlyCustomerStats(List<ApplicationUser> customers)
    {
        return customers
            .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
            .Select(g => new MonthlyCustomerStatsDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM"),
                NewCustomers = g.Count(),
                ActiveCustomers = g.Count(c => c.IsActive),
                ChurnedCustomers = g.Count(c => !c.IsActive),
                RetentionRate = g.Any() ? (double)g.Count(c => c.IsActive) / g.Count() * 100 : 0
            })
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month)
            .ToList();
    }

    private List<CustomerLanguageStatsDto> GenerateLanguageStats(List<ApplicationUser> customers)
    {
        return customers
            .GroupBy(c => c.PreferredLanguage)
            .Select(g => new CustomerLanguageStatsDto
            {
                Language = g.Key,
                LanguageName = g.Key.ToString(),
                CustomerCount = g.Count(),
                Percentage = customers.Any() ? (double)g.Count() / customers.Count * 100 : 0
            })
            .ToList();
    }

    private List<TopCustomerDto> GenerateTopCustomers(List<ApplicationUser> customers, int count)
    {
        return customers
            .Select(c => new TopCustomerDto
            {
                CustomerId = c.Id,
                CustomerName = c.FullName,
                Email = c.Email ?? "",
                TotalBookings = c.Bookings?.Count ?? 0,
                TotalSpent = c.Bookings?.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.FinalAmount) ?? 0,
                LoyaltyPoints = c.AvailableLoyaltyPoints,
                LastBookingDate = c.Bookings?.OrderByDescending(b => b.CreatedAt).FirstOrDefault()?.CreatedAt ?? DateTime.MinValue,
                CustomerSegment = DetermineCustomerSegment(c)
            })
            .OrderByDescending(c => c.TotalSpent)
            .Take(count)
            .ToList();
    }

    #endregion
}
