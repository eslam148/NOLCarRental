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
using NOL.Domain.Extensions;
using NOL.Infrastructure.Data;

namespace NOL.Infrastructure.Services;

public class BookingManagementService : IBookingManagementService
{
    private readonly ApplicationDbContext _context;
    private readonly IBookingRepository _bookingRepository;
    private readonly ICarRepository _carRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<BookingManagementService> _logger;

    public BookingManagementService(
        ApplicationDbContext context,
        IBookingRepository bookingRepository,
        ICarRepository carRepository,
        IBranchRepository branchRepository,
        UserManager<ApplicationUser> userManager,
        ILogger<BookingManagementService> logger)
    {
        _context = context;
        _bookingRepository = bookingRepository;
        _carRepository = carRepository;
        _branchRepository = branchRepository;
        _userManager = userManager;
        _logger = logger;
    }

    #region CRUD Operations

    public async Task<ApiResponse<AdminBookingDto>> GetBookingByIdAsync(int id)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Car)
                .Include(b => b.ReceivingBranch)
                .Include(b => b.DeliveryBranch)
                .Include(b => b.BookingExtras)
                    .ThenInclude(be => be.ExtraTypePrice)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return ApiResponse<AdminBookingDto>.Error("Booking not found", (string?)null, ApiStatusCode.NotFound);
            }

            var adminBookingDto = await MapToAdminBookingDto(booking);
            return ApiResponse<AdminBookingDto>.Success(adminBookingDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting booking by ID: {BookingId}", id);
            return ApiResponse<AdminBookingDto>.Error("An error occurred while retrieving booking", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<AdminBookingDto>>> GetBookingsAsync(BookingFilterDto filter)
    {
        try
        {
            var query = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Car)
                .Include(b => b.ReceivingBranch)
                .Include(b => b.DeliveryBranch)
                .Include(b => b.BookingExtras)
                    .ThenInclude(be => be.ExtraTypePrice)
                .Include(b => b.Payments)
                .AsQueryable();

            // Apply filters
            if (filter.Status.HasValue)
            {
                query = query.Where(b => b.Status == filter.Status.Value);
            }

            if (filter.StartDateFrom.HasValue)
            {
                query = query.Where(b => b.StartDate >= filter.StartDateFrom.Value);
            }

            if (filter.StartDateTo.HasValue)
            {
                query = query.Where(b => b.StartDate <= filter.StartDateTo.Value);
            }

            if (filter.EndDateFrom.HasValue)
            {
                query = query.Where(b => b.EndDate >= filter.EndDateFrom.Value);
            }

            if (filter.EndDateTo.HasValue)
            {
                query = query.Where(b => b.EndDate <= filter.EndDateTo.Value);
            }

            if (filter.CreatedDateFrom.HasValue)
            {
                query = query.Where(b => b.CreatedAt >= filter.CreatedDateFrom.Value);
            }

            if (filter.CreatedDateTo.HasValue)
            {
                query = query.Where(b => b.CreatedAt <= filter.CreatedDateTo.Value);
            }

            if (!string.IsNullOrEmpty(filter.CustomerName))
            {
                query = query.Where(b => b.User.FullName.Contains(filter.CustomerName));
            }

            if (!string.IsNullOrEmpty(filter.CustomerEmail))
            {
                query = query.Where(b => b.User.Email!.Contains(filter.CustomerEmail));
            }

            if (!string.IsNullOrEmpty(filter.BookingNumber))
            {
                query = query.Where(b => b.BookingNumber.Contains(filter.BookingNumber));
            }

            if (filter.CarId.HasValue)
            {
                query = query.Where(b => b.CarId == filter.CarId.Value);
            }

            if (filter.BranchId.HasValue)
            {
                query = query.Where(b => b.ReceivingBranchId == filter.BranchId.Value || b.DeliveryBranchId == filter.BranchId.Value);
            }

            if (filter.CategoryId.HasValue)
            {
                query = query.Where(b => b.Car.CategoryId == filter.CategoryId.Value);
            }

            if (filter.MinAmount.HasValue)
            {
                query = query.Where(b => b.FinalAmount >= filter.MinAmount.Value);
            }

            if (filter.MaxAmount.HasValue)
            {
                query = query.Where(b => b.FinalAmount <= filter.MaxAmount.Value);
            }

            if (filter.PaymentStatus.HasValue)
            {
                query = query.Where(b => b.Payments.Any(p => p.Status == filter.PaymentStatus.Value));
            }

            // Apply sorting
            query = filter.SortBy?.ToLower() switch
            {
                "bookingnumber" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(b => b.BookingNumber) : query.OrderByDescending(b => b.BookingNumber),
                "customername" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(b => b.User.FullName) : query.OrderByDescending(b => b.User.FullName),
                "startdate" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(b => b.StartDate) : query.OrderByDescending(b => b.StartDate),
                "enddate" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(b => b.EndDate) : query.OrderByDescending(b => b.EndDate),
                "totalcost" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(b => b.FinalAmount) : query.OrderByDescending(b => b.FinalAmount),
                "status" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(b => b.Status) : query.OrderByDescending(b => b.Status),
                "createdat" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(b => b.CreatedAt) : query.OrderByDescending(b => b.CreatedAt),
                _ => query.OrderByDescending(b => b.CreatedAt)
            };

            // Apply pagination
            var totalCount = await query.CountAsync();
            var bookings = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var adminBookingDtos = new List<AdminBookingDto>();
            foreach (var booking in bookings)
            {
                adminBookingDtos.Add(await MapToAdminBookingDto(booking));
            }

            return ApiResponse<List<AdminBookingDto>>.Success(adminBookingDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bookings with filter");
            return ApiResponse<List<AdminBookingDto>>.Error("An error occurred while retrieving bookings", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<AdminBookingDto>> CreateBookingAsync(AdminCreateBookingDto createBookingDto, string adminId)
    {
           var tran= await _context.Database.BeginTransactionAsync();      // Validate user exists
        
        try
        {
            var user = await _userManager.FindByIdAsync(createBookingDto.UserId);
            if (user == null)
            {
                return ApiResponse<AdminBookingDto>.Error("User not found", (string?)null, ApiStatusCode.NotFound);
            }

            // Validate car exists and is available
            var car = await _carRepository.GetByIdAsync(createBookingDto.CarId);
            if (car == null)
            {
                return ApiResponse<AdminBookingDto>.Error("Car not found", (string?)null, ApiStatusCode.NotFound);
            }

            if (car.Status != CarStatus.Available)
            {
                return ApiResponse<AdminBookingDto>.Error("Car is not available", (string?)null, ApiStatusCode.BadRequest);
            }

            // Check car availability for the requested dates
            var isAvailable = await _bookingRepository.IsCarAvailableAsync(
                createBookingDto.CarId,
                createBookingDto.StartDate,
                createBookingDto.EndDate);

            if (!isAvailable)
            {
                return ApiResponse<AdminBookingDto>.Error("Car is not available for the selected dates", (string?)null, ApiStatusCode.BadRequest);
            }

            // Validate branches exist
            var receivingBranch = await _branchRepository.GetByIdAsync(createBookingDto.ReceivingBranchId);
            var deliveryBranch = await _branchRepository.GetByIdAsync(createBookingDto.DeliveryBranchId);

            if (receivingBranch == null || deliveryBranch == null)
            {
                return ApiResponse<AdminBookingDto>.Error("Invalid branch selection", (string?)null, ApiStatusCode.BadRequest);
            }

            // Calculate costs
            var totalDays = (createBookingDto.EndDate - createBookingDto.StartDate).Days + 1;
            var carRentalCost = car.DailyRate * totalDays;

            // Calculate extras cost
            decimal extrasCost = 0;
            var bookingExtras = new List<BookingExtra>();

            if (createBookingDto.Extras.Any())
            {
                var extraTypePriceIds = createBookingDto.Extras.Select(e => e.ExtraTypePriceId).ToList();
                var extraTypePrices = await _context.ExtraTypePrices
                    .Where(etp => extraTypePriceIds.Contains(etp.Id))
                    .ToListAsync();

                foreach (var extraDto in createBookingDto.Extras)
                {
                    var extraTypePrice = extraTypePrices.FirstOrDefault(etp => etp.Id == extraDto.ExtraTypePriceId);
                    if (extraTypePrice != null)
                    {
                        var extraTotalCost = extraTypePrice.DailyPrice * extraDto.Quantity * totalDays;
                        extrasCost += extraTotalCost;

                        bookingExtras.Add(new BookingExtra
                        {
                            ExtraTypePriceId = extraDto.ExtraTypePriceId,
                            Quantity = extraDto.Quantity,
                            UnitPrice = extraTypePrice.DailyPrice,
                            TotalPrice = extraTotalCost
                        });
                    }
                }
            }

            var totalCost = carRentalCost + extrasCost;
            var discountAmount = createBookingDto.DiscountAmount ?? 0;
            var finalAmount = totalCost - discountAmount;

            // Generate booking number
            var bookingNumber = GenerateBookingNumber();

            var booking = new Booking
            {
                BookingNumber = bookingNumber,
                UserId = createBookingDto.UserId,
                CarId = createBookingDto.CarId,
                ReceivingBranchId = createBookingDto.ReceivingBranchId,
                DeliveryBranchId = createBookingDto.DeliveryBranchId,
                StartDate = createBookingDto.StartDate,
                EndDate = createBookingDto.EndDate,
                TotalDays = totalDays,
                CarRentalCost = carRentalCost,
                ExtrasCost = extrasCost,
                TotalCost = totalCost,
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount,
                Status = BookingStatus.Open,
                Notes = createBookingDto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                BookingExtras = bookingExtras
            };

            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();

            await tran.CommitAsync();      // Validate user exists

            // Get the created booking with includes
            var createdBooking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Car)
                .Include(b => b.ReceivingBranch)
                .Include(b => b.DeliveryBranch)
                .Include(b => b.BookingExtras)
                    .ThenInclude(be => be.ExtraTypePrice)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == booking.Id);

            var adminBookingDto = await MapToAdminBookingDto(createdBooking!);

            _logger.LogInformation("Booking created successfully by admin {AdminId}: {BookingId}", adminId, booking.Id);
            return ApiResponse<AdminBookingDto>.Success(adminBookingDto);
        }
        catch (Exception ex)
        {
            await tran.RollbackAsync();
            _logger.LogError(ex, "Error creating booking by admin {AdminId}", adminId);
            return ApiResponse<AdminBookingDto>.Error("An error occurred while creating booking", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<AdminBookingDto>> UpdateBookingAsync(int id, ModifyBookingDto modifyBookingDto, string adminId)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Car)
                .Include(b => b.ReceivingBranch)
                .Include(b => b.DeliveryBranch)
                .Include(b => b.BookingExtras)
                    .ThenInclude(be => be.ExtraTypePrice)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return ApiResponse<AdminBookingDto>.Error("Booking not found", (string?)null, ApiStatusCode.NotFound);
            }

            // Check if booking can be modified
            if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Canceled)
            {
                return ApiResponse<AdminBookingDto>.Error("Cannot modify completed or canceled booking", (string?)null, ApiStatusCode.BadRequest);
            }

            var needsRecalculation = false;

            // Update dates if provided
            if (modifyBookingDto.StartDate.HasValue && modifyBookingDto.StartDate.Value != booking.StartDate)
            {
                // Check availability for new dates
                var isAvailable = await _bookingRepository.IsCarAvailableAsync(
                    booking.CarId,
                    modifyBookingDto.StartDate.Value,
                    modifyBookingDto.EndDate ?? booking.EndDate,
                    booking.Id);

                if (!isAvailable)
                {
                    return ApiResponse<AdminBookingDto>.Error("Car is not available for the new dates", (string?)null, ApiStatusCode.BadRequest);
                }

                booking.StartDate = modifyBookingDto.StartDate.Value;
                needsRecalculation = true;
            }

            if (modifyBookingDto.EndDate.HasValue && modifyBookingDto.EndDate.Value != booking.EndDate)
            {
                // Check availability for new dates
                var isAvailable = await _bookingRepository.IsCarAvailableAsync(
                    booking.CarId,
                    booking.StartDate,
                    modifyBookingDto.EndDate.Value,
                    booking.Id);

                if (!isAvailable)
                {
                    return ApiResponse<AdminBookingDto>.Error("Car is not available for the new dates", (string?)null, ApiStatusCode.BadRequest);
                }

                booking.EndDate = modifyBookingDto.EndDate.Value;
                needsRecalculation = true;
            }

            // Update branches if provided
            if (modifyBookingDto.ReceivingBranchId.HasValue)
            {
                var branch = await _branchRepository.GetByIdAsync(modifyBookingDto.ReceivingBranchId.Value);
                if (branch == null)
                {
                    return ApiResponse<AdminBookingDto>.Error("Invalid receiving branch", (string?)null, ApiStatusCode.BadRequest);
                }
                booking.ReceivingBranchId = modifyBookingDto.ReceivingBranchId.Value;
            }

            if (modifyBookingDto.DeliveryBranchId.HasValue)
            {
                var branch = await _branchRepository.GetByIdAsync(modifyBookingDto.DeliveryBranchId.Value);
                if (branch == null)
                {
                    return ApiResponse<AdminBookingDto>.Error("Invalid delivery branch", (string?)null, ApiStatusCode.BadRequest);
                }
                booking.DeliveryBranchId = modifyBookingDto.DeliveryBranchId.Value;
            }

            // Update extras if provided
            if (modifyBookingDto.Extras != null)
            {
                // Remove existing extras
                _context.BookingExtras.RemoveRange(booking.BookingExtras);

                // Add new extras
                var extraTypePriceIds = modifyBookingDto.Extras.Select(e => e.ExtraTypePriceId).ToList();
                var extraTypePrices = await _context.ExtraTypePrices
                    .Where(etp => extraTypePriceIds.Contains(etp.Id))
                    .ToListAsync();

                var newBookingExtras = new List<BookingExtra>();
                foreach (var extraDto in modifyBookingDto.Extras)
                {
                    var extraTypePrice = extraTypePrices.FirstOrDefault(etp => etp.Id == extraDto.ExtraTypePriceId);
                    if (extraTypePrice != null)
                    {
                        var totalDays = (booking.EndDate - booking.StartDate).Days + 1;
                        var extraTotalCost = extraTypePrice.DailyPrice * extraDto.Quantity * totalDays;

                        newBookingExtras.Add(new BookingExtra
                        {
                            BookingId = booking.Id,
                            ExtraTypePriceId = extraDto.ExtraTypePriceId,
                            Quantity = extraDto.Quantity,
                            UnitPrice = extraTypePrice.DailyPrice,
                            TotalPrice = extraTotalCost
                        });
                    }
                }

                booking.BookingExtras = newBookingExtras;
                needsRecalculation = true;
            }

            // Update other fields
            if (modifyBookingDto.Notes != null)
            {
                booking.Notes = modifyBookingDto.Notes;
            }

            if (modifyBookingDto.DiscountAmount.HasValue)
            {
                booking.DiscountAmount = modifyBookingDto.DiscountAmount.Value;
                needsRecalculation = true;
            }

            // Recalculate costs if needed
            if (needsRecalculation)
            {
                var totalDays = (booking.EndDate - booking.StartDate).Days + 1;
                booking.TotalDays = totalDays;
                booking.CarRentalCost = booking.Car.DailyRate * totalDays;
                booking.ExtrasCost = booking.BookingExtras.Sum(be => be.TotalPrice);
                booking.TotalCost = booking.CarRentalCost + booking.ExtrasCost;
                booking.FinalAmount = booking.TotalCost - booking.DiscountAmount;
            }

            booking.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Get updated booking with includes
            var updatedBooking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Car)
                .Include(b => b.ReceivingBranch)
                .Include(b => b.DeliveryBranch)
                .Include(b => b.BookingExtras)
                    .ThenInclude(be => be.ExtraTypePrice)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == id);

            var adminBookingDto = await MapToAdminBookingDto(updatedBooking!);

            _logger.LogInformation("Booking updated successfully by admin {AdminId}: {BookingId}", adminId, id);
            return ApiResponse<AdminBookingDto>.Success(adminBookingDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating booking {BookingId} by admin {AdminId}", id, adminId);
            return ApiResponse<AdminBookingDto>.Error("An error occurred while updating booking", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> DeleteBookingAsync(int id, string adminId)
    {
        try
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return ApiResponse.Error("Booking not found", (string?)null, ApiStatusCode.NotFound);
            }

            // Check if booking can be deleted
            if (booking.Status == BookingStatus.InProgress || booking.Status == BookingStatus.Completed)
            {
                return ApiResponse.Error("Cannot delete in-progress or completed booking", (string?)null, ApiStatusCode.BadRequest);
            }

            // Remove related entities
            var bookingExtras = await _context.BookingExtras.Where(be => be.BookingId == id).ToListAsync();
            var payments = await _context.Payments.Where(p => p.BookingId == id).ToListAsync();

            _context.BookingExtras.RemoveRange(bookingExtras);
            _context.Payments.RemoveRange(payments);
            _context.Bookings.Remove(booking);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Booking deleted successfully by admin {AdminId}: {BookingId}", adminId, id);
            return ApiResponse.Success("Booking deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting booking {BookingId} by admin {AdminId}", id, adminId);
            return ApiResponse.Error("An error occurred while deleting booking", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Status Management

    public async Task<ApiResponse<AdminBookingDto>> UpdateBookingStatusAsync(int id, UpdateBookingStatusDto statusDto, string adminId)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Car)
                .Include(b => b.ReceivingBranch)
                .Include(b => b.DeliveryBranch)
                .Include(b => b.BookingExtras)
                    .ThenInclude(be => be.ExtraTypePrice)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return ApiResponse<AdminBookingDto>.Error("Booking not found", (string?)null, ApiStatusCode.NotFound);
            }

            var oldStatus = booking.Status;
            booking.Status = statusDto.Status;
            booking.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(statusDto.Notes))
            {
                booking.Notes = statusDto.Notes;
            }

            if (statusDto.Status == BookingStatus.Canceled && !string.IsNullOrEmpty(statusDto.CancellationReason))
            {
                booking.CancellationReason = statusDto.CancellationReason;
            }

            // Update car status based on booking status
            if (statusDto.Status == BookingStatus.InProgress)
            {
                booking.Car.Status = CarStatus.Rented;
            }
            else if (statusDto.Status == BookingStatus.Completed || statusDto.Status == BookingStatus.Canceled)
            {
                booking.Car.Status = CarStatus.Available;
            }

            await _context.SaveChangesAsync();

            var adminBookingDto = await MapToAdminBookingDto(booking);

            _logger.LogInformation("Booking status updated by admin {AdminId}: {BookingId} from {OldStatus} to {NewStatus}",
                adminId, id, oldStatus, statusDto.Status);

            return ApiResponse<AdminBookingDto>.Success(adminBookingDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating booking status {BookingId} by admin {AdminId}", id, adminId);
            return ApiResponse<AdminBookingDto>.Error("An error occurred while updating booking status", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse> BulkUpdateBookingStatusAsync(List<int> bookingIds, BookingStatus status, string adminId, string? notes = null)
    {
        try
        {
            var bookings = await _context.Bookings
                .Include(b => b.Car)
                .Where(b => bookingIds.Contains(b.Id))
                .ToListAsync();

            if (!bookings.Any())
            {
                return ApiResponse.Error("No valid bookings found", (string?)null, ApiStatusCode.NotFound);
            }

            foreach (var booking in bookings)
            {
                booking.Status = status;
                booking.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(notes))
                {
                    booking.Notes = notes;
                }

                // Update car status based on booking status
                if (status == BookingStatus.InProgress)
                {
                    booking.Car.Status = CarStatus.Rented;
                }
                else if (status == BookingStatus.Completed || status == BookingStatus.Canceled)
                {
                    booking.Car.Status = CarStatus.Available;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Bulk booking status update by admin {AdminId}: {BookingCount} bookings updated to {Status}",
                adminId, bookings.Count, status);

            return ApiResponse.Success($"Successfully updated {bookings.Count} booking(s) status to {status}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating booking status by admin {AdminId}", adminId);
            return ApiResponse.Error("An error occurred while updating booking status", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Booking Workflow

    public async Task<ApiResponse<AdminBookingDto>> ConfirmBookingAsync(int id, string adminId)
    {
        try
        {
            var statusDto = new UpdateBookingStatusDto
            {
                Status = BookingStatus.Confirmed,
                Notes = "Booking confirmed by admin"
            };

            return await UpdateBookingStatusAsync(id, statusDto, adminId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming booking {BookingId} by admin {AdminId}", id, adminId);
            return ApiResponse<AdminBookingDto>.Error("An error occurred while confirming booking", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<AdminBookingDto>> StartBookingAsync(int id, string adminId)
    {
        try
        {
            var statusDto = new UpdateBookingStatusDto
            {
                Status = BookingStatus.InProgress,
                Notes = "Booking started by admin"
            };

            return await UpdateBookingStatusAsync(id, statusDto, adminId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting booking {BookingId} by admin {AdminId}", id, adminId);
            return ApiResponse<AdminBookingDto>.Error("An error occurred while starting booking", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<AdminBookingDto>> CompleteBookingAsync(int id, string adminId)
    {
        try
        {
            var statusDto = new UpdateBookingStatusDto
            {
                Status = BookingStatus.Completed,
                Notes = "Booking completed by admin"
            };

            return await UpdateBookingStatusAsync(id, statusDto, adminId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing booking {BookingId} by admin {AdminId}", id, adminId);
            return ApiResponse<AdminBookingDto>.Error("An error occurred while completing booking", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<AdminBookingDto>> CancelBookingAsync(int id, string cancellationReason, string adminId)
    {
        try
        {
            var statusDto = new UpdateBookingStatusDto
            {
                Status = BookingStatus.Canceled,
                CancellationReason = cancellationReason,
                Notes = "Booking canceled by admin"
            };

            return await UpdateBookingStatusAsync(id, statusDto, adminId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling booking {BookingId} by admin {AdminId}", id, adminId);
            return ApiResponse<AdminBookingDto>.Error("An error occurred while canceling booking", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Bulk Operations

    public async Task<ApiResponse> BulkOperationAsync(BulkBookingOperationDto operationDto, string adminId)
    {
        try
        {
            switch (operationDto.Operation.ToLower())
            {
                case "cancel":
                    return await BulkCancelBookingsAsync(operationDto.BookingIds, operationDto.Reason ?? "Bulk cancellation", adminId);

                case "confirm":
                    return await BulkUpdateBookingStatusAsync(operationDto.BookingIds, BookingStatus.Confirmed, adminId, operationDto.Notes);

                case "complete":
                    return await BulkUpdateBookingStatusAsync(operationDto.BookingIds, BookingStatus.Completed, adminId, operationDto.Notes);

                case "updatestatus":
                    if (!operationDto.NewStatus.HasValue)
                        return ApiResponse.Error("New status is required for status update operation", (string?)null, ApiStatusCode.BadRequest);
                    return await BulkUpdateBookingStatusAsync(operationDto.BookingIds, operationDto.NewStatus.Value, adminId, operationDto.Notes);

                default:
                    return ApiResponse.Error("Invalid operation", (string?)null, ApiStatusCode.BadRequest);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing bulk operation by admin {AdminId}", adminId);
            return ApiResponse.Error("An error occurred while performing bulk operation", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    private async Task<ApiResponse> BulkCancelBookingsAsync(List<int> bookingIds, string cancellationReason, string adminId)
    {
        try
        {
            var bookings = await _context.Bookings
                .Include(b => b.Car)
                .Where(b => bookingIds.Contains(b.Id))
                .ToListAsync();

            if (!bookings.Any())
            {
                return ApiResponse.Error("No valid bookings found", (string?)null, ApiStatusCode.NotFound);
            }

            foreach (var booking in bookings)
            {
                booking.Status = BookingStatus.Canceled;
                booking.CancellationReason = cancellationReason;
                booking.UpdatedAt = DateTime.UtcNow;
                booking.Car.Status = CarStatus.Available;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Bulk booking cancellation by admin {AdminId}: {BookingCount} bookings canceled",
                adminId, bookings.Count);

            return ApiResponse.Success($"Successfully canceled {bookings.Count} booking(s)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk canceling bookings by admin {AdminId}", adminId);
            return ApiResponse.Error("An error occurred while canceling bookings", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Analytics and Reporting

    public async Task<ApiResponse<BookingAnalyticsDto>> GetBookingAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null, int? branchId = null)
    {
        try
        {
            var query = _context.Bookings.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(b => b.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(b => b.CreatedAt <= endDate.Value);

            if (branchId.HasValue)
                query = query.Where(b => b.ReceivingBranchId == branchId.Value || b.DeliveryBranchId == branchId.Value);

            var bookings = await query.ToListAsync();

            var analytics = new BookingAnalyticsDto
            {
                TotalBookings = bookings.Count,
                TotalRevenue = bookings.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.FinalAmount),
                AverageBookingValue = bookings.Any() ? bookings.Average(b => b.FinalAmount) : 0,
                AverageBookingDuration = bookings.Any() ? bookings.Average(b => (double)b.TotalDays) : 0,
                StatusBreakdown = GenerateStatusBreakdown(bookings),
                MonthlyStats = GenerateMonthlyStats(bookings),
                PopularCars = await GeneratePopularCarsStats(bookings),
                BranchStats = await GenerateBranchStats(bookings),
                PeakTimes = GeneratePeakTimesStats(bookings)
            };

            return ApiResponse<BookingAnalyticsDto>.Success(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting booking analytics");
            return ApiResponse<BookingAnalyticsDto>.Error("An error occurred while retrieving booking analytics", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<BookingReportDto>> GenerateBookingReportAsync(BookingFilterDto filter)
    {
        try
        {
            var bookingsResponse = await GetBookingsAsync(filter);
            if (!bookingsResponse.Succeeded || bookingsResponse.Data == null)
            {
                return ApiResponse<BookingReportDto>.Error("Failed to retrieve bookings for report", (string?)null, ApiStatusCode.InternalServerError);
            }

            var analytics = await GetBookingAnalyticsAsync(filter.CreatedDateFrom, filter.CreatedDateTo, filter.BranchId);

            var report = new BookingReportDto
            {
                GeneratedAt = DateTime.UtcNow,
                StartDate = filter.CreatedDateFrom,
                EndDate = filter.CreatedDateTo,
                Analytics = analytics.Data ?? new BookingAnalyticsDto(),
                Bookings = bookingsResponse.Data,
                Summary = new BookingReportSummaryDto
                {
                    TotalBookings = bookingsResponse.Data.Count,
                    CompletedBookings = bookingsResponse.Data.Count(b => b.Status == BookingStatus.Completed),
                    CancelledBookings = bookingsResponse.Data.Count(b => b.Status == BookingStatus.Canceled),
                    TotalRevenue = bookingsResponse.Data.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.FinalAmount),
                    AverageBookingValue = bookingsResponse.Data.Any() ? bookingsResponse.Data.Average(b => b.FinalAmount) : 0,
                    CancellationRate = bookingsResponse.Data.Any() ?
                        (double)bookingsResponse.Data.Count(b => b.Status == BookingStatus.Canceled) / bookingsResponse.Data.Count * 100 : 0
                }
            };

            return ApiResponse<BookingReportDto>.Success(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating booking report");
            return ApiResponse<BookingReportDto>.Error("An error occurred while generating report", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<byte[]>> ExportBookingReportAsync(BookingFilterDto filter, string format = "excel")
    {
        try
        {
            // For this implementation, return empty byte array as export functionality would require additional libraries
            // In a real implementation, you would use libraries like EPPlus for Excel or similar
            _logger.LogInformation("Booking report export requested with format: {Format}", format);
            return ApiResponse<byte[]>.Success(Array.Empty<byte>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting booking report");
            return ApiResponse<byte[]>.Error("An error occurred while exporting report", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Payment Management

    public async Task<ApiResponse<List<PaymentDetailDto>>> GetBookingPaymentsAsync(int bookingId)
    {
        try
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null)
            {
                return ApiResponse<List<PaymentDetailDto>>.Error("Booking not found", (string?)null, ApiStatusCode.NotFound);
            }

            var payments = await _context.Payments
                .Where(p => p.BookingId == bookingId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            var paymentDtos = payments.Select(p => new PaymentDetailDto
            {
                Id = p.Id,
                PaymentReference = p.PaymentReference,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                Status = p.Status,
                PaymentDate = p.PaymentDate,
                TransactionId = p.TransactionId,
                Notes = p.Notes
            }).ToList();

            return ApiResponse<List<PaymentDetailDto>>.Success(paymentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting booking payments: {BookingId}", bookingId);
            return ApiResponse<List<PaymentDetailDto>>.Error("An error occurred while retrieving payments", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<PaymentDetailDto>> AddBookingPaymentAsync(int bookingId, PaymentDetailDto paymentDto, string adminId)
    {
        try
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null)
            {
                return ApiResponse<PaymentDetailDto>.Error("Booking not found", (string?)null, ApiStatusCode.NotFound);
            }

            var payment = new Payment
            {
                BookingId = bookingId,
                PaymentReference = paymentDto.PaymentReference,
                Amount = paymentDto.Amount,
                PaymentMethod = paymentDto.PaymentMethod,
                Status = paymentDto.Status,
                PaymentDate = paymentDto.PaymentDate,
                TransactionId = paymentDto.TransactionId,
                Notes = paymentDto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();

            var resultDto = new PaymentDetailDto
            {
                Id = payment.Id,
                PaymentReference = payment.PaymentReference,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                Status = payment.Status,
                PaymentDate = payment.PaymentDate,
                TransactionId = payment.TransactionId,
                Notes = payment.Notes
            };

            _logger.LogInformation("Payment added to booking {BookingId} by admin {AdminId}: {PaymentId}", bookingId, adminId, payment.Id);
            return ApiResponse<PaymentDetailDto>.Success(resultDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding payment to booking {BookingId} by admin {AdminId}", bookingId, adminId);
            return ApiResponse<PaymentDetailDto>.Error("An error occurred while adding payment", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<PaymentDetailDto>> UpdatePaymentStatusAsync(int paymentId, PaymentStatus status, string adminId)
    {
        try
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
            {
                return ApiResponse<PaymentDetailDto>.Error("Payment not found", (string?)null, ApiStatusCode.NotFound);
            }

            payment.Status = status;
            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var resultDto = new PaymentDetailDto
            {
                Id = payment.Id,
                PaymentReference = payment.PaymentReference,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                Status = payment.Status,
                PaymentDate = payment.PaymentDate,
                TransactionId = payment.TransactionId,
                Notes = payment.Notes
            };

            _logger.LogInformation("Payment status updated by admin {AdminId}: {PaymentId} to {Status}", adminId, paymentId, status);
            return ApiResponse<PaymentDetailDto>.Success(resultDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment status {PaymentId} by admin {AdminId}", paymentId, adminId);
            return ApiResponse<PaymentDetailDto>.Error("An error occurred while updating payment status", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Revenue Tracking

    public async Task<ApiResponse<decimal>> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null, int? branchId = null)
    {
        try
        {
            var query = _context.Bookings
                .Where(b => b.Status == BookingStatus.Completed)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(b => b.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(b => b.CreatedAt <= endDate.Value);

            if (branchId.HasValue)
                query = query.Where(b => b.ReceivingBranchId == branchId.Value || b.DeliveryBranchId == branchId.Value);

            var totalRevenue = await query.SumAsync(b => b.FinalAmount);

            return ApiResponse<decimal>.Success(totalRevenue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total revenue");
            return ApiResponse<decimal>.Error("An error occurred while retrieving total revenue", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<MonthlyBookingStatsDto>>> GetMonthlyRevenueAsync(int year, int? branchId = null)
    {
        try
        {
            var query = _context.Bookings
                .Where(b => b.CreatedAt.Year == year)
                .AsQueryable();

            if (branchId.HasValue)
                query = query.Where(b => b.ReceivingBranchId == branchId.Value || b.DeliveryBranchId == branchId.Value);

            var bookings = await query.ToListAsync();

            var monthlyStats = bookings
                .GroupBy(b => b.CreatedAt.Month)
                .Select(g => new MonthlyBookingStatsDto
                {
                    Year = year,
                    Month = g.Key,
                    MonthName = new DateTime(year, g.Key, 1).ToString("MMMM"),
                    BookingCount = g.Count(),
                    Revenue = g.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.FinalAmount),
                    CompletedBookings = g.Count(b => b.Status == BookingStatus.Completed),
                    CancelledBookings = g.Count(b => b.Status == BookingStatus.Canceled),
                    CancellationRate = g.Any() ? (double)g.Count(b => b.Status == BookingStatus.Canceled) / g.Count() * 100 : 0
                })
                .OrderBy(m => m.Month)
                .ToList();

            return ApiResponse<List<MonthlyBookingStatsDto>>.Success(monthlyStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting monthly revenue for year {Year}", year);
            return ApiResponse<List<MonthlyBookingStatsDto>>.Error("An error occurred while retrieving monthly revenue", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<DailyRevenueDto>>> GetDailyRevenueAsync(DateTime startDate, DateTime endDate, int? branchId = null)
    {
        try
        {
            var query = _context.Bookings
                .Where(b => b.CreatedAt.Date >= startDate.Date && b.CreatedAt.Date <= endDate.Date)
                .AsQueryable();

            if (branchId.HasValue)
                query = query.Where(b => b.ReceivingBranchId == branchId.Value || b.DeliveryBranchId == branchId.Value);

            var bookings = await query.ToListAsync();

            var dailyRevenue = bookings
                .GroupBy(b => b.CreatedAt.Date)
                .Select(g => new DailyRevenueDto
                {
                    Date = g.Key,
                    BookingCount = g.Count(),
                    Revenue = g.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.FinalAmount)
                })
                .OrderBy(d => d.Date)
                .ToList();

            return ApiResponse<List<DailyRevenueDto>>.Success(dailyRevenue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting daily revenue from {StartDate} to {EndDate}", startDate, endDate);
            return ApiResponse<List<DailyRevenueDto>>.Error("An error occurred while retrieving daily revenue", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    // Helper method to generate booking number
    private string GenerateBookingNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(1000, 9999);
        return $"BK{timestamp}{random}";
    }

    // Helper method to map Booking entity to AdminBookingDto
    private Task<AdminBookingDto> MapToAdminBookingDto(Booking booking)
    {
        var extrasDetails = booking.BookingExtras?.Select(be => new BookingExtraDetailDto
        {
            Id = be.Id,
            ExtraName = be.ExtraTypePrice?.NameEn ?? "",
            Quantity = be.Quantity,
            UnitPrice = be.UnitPrice,
            TotalPrice = be.TotalPrice
        }).ToList() ?? new List<BookingExtraDetailDto>();

        var paymentDetails = booking.Payments?.Select(p => new PaymentDetailDto
        {
            Id = p.Id,
            PaymentReference = p.PaymentReference,
            Amount = p.Amount,
            PaymentMethod = p.PaymentMethod,
            Status = p.Status,
            PaymentDate = p.PaymentDate,
            TransactionId = p.TransactionId,
            Notes = p.Notes
        }).ToList() ?? new List<PaymentDetailDto>();

        return Task.FromResult(new AdminBookingDto
        {
            Id = booking.Id,
            BookingNumber = booking.BookingNumber,
            StartDate = booking.StartDate,
            EndDate = booking.EndDate,
            TotalDays = booking.TotalDays,
            CarRentalCost = booking.CarRentalCost,
            ExtrasCost = booking.ExtrasCost,
            TotalCost = booking.TotalCost,
            DiscountAmount = booking.DiscountAmount,
            FinalAmount = booking.FinalAmount,
            Status = booking.Status,
            Notes = booking.Notes,
            CancellationReason = booking.CancellationReason,
            CreatedAt = booking.CreatedAt,
            UpdatedAt = booking.UpdatedAt,
            CustomerName = booking.User?.FullName ?? "",
            CustomerEmail = booking.User?.Email ?? "",
            CustomerPhone = booking.User?.PhoneNumber ?? "",
            CarBrand = booking.Car?.BrandEn ?? "",
            CarModel = booking.Car?.ModelEn ?? "",
            CarPlateNumber = booking.Car?.PlateNumber ?? "",
            ReceivingBranchName = booking.ReceivingBranch?.NameEn ?? "",
            DeliveryBranchName = booking.DeliveryBranch?.NameEn ?? "",
            ExtrasDetails = extrasDetails,
            PaymentDetails = paymentDetails,
            Car = new CarDto
            {
                Id = booking.Car?.Id ?? 0,
                Brand = booking.Car?.BrandEn ?? "",
                Model = booking.Car?.ModelEn ?? "",
                Year = booking.Car?.Year ?? 0,
                Color = booking.Car?.ColorEn ?? "",
                SeatingCapacity = booking.Car?.SeatingCapacity ?? 0,
                TransmissionType = booking.Car?.TransmissionType.ToString() ?? "",
                FuelType = booking.Car?.FuelType ?? FuelType.Gasoline,
                DailyPrice = booking.Car?.DailyRate ?? 0,
                WeeklyPrice = booking.Car?.WeeklyRate ?? 0,
                MonthlyPrice = booking.Car?.MonthlyRate ?? 0,
                Status = booking.Car?.Status.GetDescription() ?? CarStatus.Available.GetDescription(),
                ImageUrl = booking.Car?.ImageUrl,
                Description = booking.Car?.DescriptionEn,
                Mileage = booking.Car?.Mileage ?? 0
            },
            ReceivingBranch = new BranchDto
            {
                Id = booking.ReceivingBranch?.Id ?? 0,
                Name = booking.ReceivingBranch?.NameEn ?? "",
                Description = booking.ReceivingBranch?.DescriptionEn ?? "",
                Address = booking.ReceivingBranch?.Address ?? "",
                City = booking.ReceivingBranch?.City ?? "",
                Country = booking.ReceivingBranch?.Country ?? "",
                Phone = booking.ReceivingBranch?.Phone,
                Email = booking.ReceivingBranch?.Email,
                Latitude = booking.ReceivingBranch?.Latitude ?? 0,
                Longitude = booking.ReceivingBranch?.Longitude ?? 0,
                WorkingHours = booking.ReceivingBranch?.WorkingHours
            },
            DeliveryBranch = new BranchDto
            {
                Id = booking.DeliveryBranch?.Id ?? 0,
                Name = booking.DeliveryBranch?.NameEn ?? "",
                Description = booking.DeliveryBranch?.DescriptionEn ?? "",
                Address = booking.DeliveryBranch?.Address ?? "",
                City = booking.DeliveryBranch?.City ?? "",
                Country = booking.DeliveryBranch?.Country ?? "",
                Phone = booking.DeliveryBranch?.Phone,
                Email = booking.DeliveryBranch?.Email,
                Latitude = booking.DeliveryBranch?.Latitude ?? 0,
                Longitude = booking.DeliveryBranch?.Longitude ?? 0,
                WorkingHours = booking.DeliveryBranch?.WorkingHours
            },
            Extras = extrasDetails.Select(ed => new BookingExtraDto
            {
                Id = ed.Id,
                ExtraName = ed.ExtraName,
                Quantity = ed.Quantity,
                UnitPrice = ed.UnitPrice,
                TotalPrice = ed.TotalPrice
            }).ToList()
        });
    }

    #region Peak Time Analysis and Customer Management

    public async Task<ApiResponse<List<PeakTimeStatsDto>>> GetPeakTimesAnalysisAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.Bookings.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(b => b.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(b => b.CreatedAt <= endDate.Value);

            var bookings = await query.ToListAsync();
            var peakTimes = GeneratePeakTimesStats(bookings);

            return ApiResponse<List<PeakTimeStatsDto>>.Success(peakTimes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting peak times analysis");
            return ApiResponse<List<PeakTimeStatsDto>>.Error("An error occurred while retrieving peak times analysis", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<PopularCarStatsDto>>> GetPopularCarsAnalysisAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.Bookings
                .Include(b => b.Car)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(b => b.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(b => b.CreatedAt <= endDate.Value);

            var bookings = await query.ToListAsync();
            var popularCars = await GeneratePopularCarsStats(bookings);

            return ApiResponse<List<PopularCarStatsDto>>.Success(popularCars);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting popular cars analysis");
            return ApiResponse<List<PopularCarStatsDto>>.Error("An error occurred while retrieving popular cars analysis", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<BranchBookingStatsDto>>> GetBranchPerformanceAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.Bookings
                .Include(b => b.ReceivingBranch)
                .Include(b => b.DeliveryBranch)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(b => b.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(b => b.CreatedAt <= endDate.Value);

            var bookings = await query.ToListAsync();
            var branchStats = await GenerateBranchStats(bookings);

            return ApiResponse<List<BranchBookingStatsDto>>.Success(branchStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting branch performance");
            return ApiResponse<List<BranchBookingStatsDto>>.Error("An error occurred while retrieving branch performance", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<AdminBookingDto>>> GetCustomerBookingsAsync(string customerId, BookingFilterDto? filter = null)
    {
        try
        {
            var query = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Car)
                .Include(b => b.ReceivingBranch)
                .Include(b => b.DeliveryBranch)
                .Include(b => b.BookingExtras)
                    .ThenInclude(be => be.ExtraTypePrice)
                .Include(b => b.Payments)
                .Where(b => b.UserId == customerId)
                .AsQueryable();

            // Apply additional filters if provided
            if (filter != null)
            {
                if (filter.Status.HasValue)
                    query = query.Where(b => b.Status == filter.Status.Value);

                if (filter.StartDateFrom.HasValue)
                    query = query.Where(b => b.StartDate >= filter.StartDateFrom.Value);

                if (filter.StartDateTo.HasValue)
                    query = query.Where(b => b.StartDate <= filter.StartDateTo.Value);

                if (filter.CarId.HasValue)
                    query = query.Where(b => b.CarId == filter.CarId.Value);
            }

            var bookings = await query
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var adminBookingDtos = new List<AdminBookingDto>();
            foreach (var booking in bookings)
            {
                adminBookingDtos.Add(await MapToAdminBookingDto(booking));
            }

            return ApiResponse<List<AdminBookingDto>>.Success(adminBookingDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer bookings: {CustomerId}", customerId);
            return ApiResponse<List<AdminBookingDto>>.Error("An error occurred while retrieving customer bookings", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<decimal>> GetCustomerTotalSpentAsync(string customerId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.Bookings
                .Where(b => b.UserId == customerId && b.Status == BookingStatus.Completed)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(b => b.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(b => b.CreatedAt <= endDate.Value);

            var totalSpent = await query.SumAsync(b => b.FinalAmount);

            return ApiResponse<decimal>.Success(totalSpent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer total spent: {CustomerId}", customerId);
            return ApiResponse<decimal>.Error("An error occurred while retrieving customer total spent", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Validation and Business Rules

    public async Task<ApiResponse<bool>> ValidateBookingDatesAsync(int carId, DateTime startDate, DateTime endDate, int? excludeBookingId = null)
    {
        try
        {
            var isAvailable = await _bookingRepository.IsCarAvailableAsync(carId, startDate, endDate, excludeBookingId);
            return ApiResponse<bool>.Success(isAvailable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating booking dates for car {CarId}", carId);
            return ApiResponse<bool>.Error("An error occurred while validating booking dates", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<decimal>> CalculateBookingCostAsync(int carId, DateTime startDate, DateTime endDate, List<AdminBookingExtraDto>? extras = null)
    {
        try
        {
            var car = await _carRepository.GetByIdAsync(carId);
            if (car == null)
            {
                return ApiResponse<decimal>.Error("Car not found", (string?)null, ApiStatusCode.NotFound);
            }

            var totalDays = (endDate - startDate).Days + 1;
            var carRentalCost = car.DailyRate * totalDays;

            decimal extrasCost = 0;
            if (extras != null && extras.Any())
            {
                var extraTypePriceIds = extras.Select(e => e.ExtraTypePriceId).ToList();
                var extraTypePrices = await _context.ExtraTypePrices
                    .Where(etp => extraTypePriceIds.Contains(etp.Id))
                    .ToListAsync();

                foreach (var extraDto in extras)
                {
                    var extraTypePrice = extraTypePrices.FirstOrDefault(etp => etp.Id == extraDto.ExtraTypePriceId);
                    if (extraTypePrice != null)
                    {
                        extrasCost += extraTypePrice.DailyPrice * extraDto.Quantity * totalDays;
                    }
                }
            }

            var totalCost = carRentalCost + extrasCost;
            return ApiResponse<decimal>.Success(totalCost);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating booking cost for car {CarId}", carId);
            return ApiResponse<decimal>.Error("An error occurred while calculating booking cost", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<int>>> GetAvailableCarsAsync(DateTime startDate, DateTime endDate, int? branchId = null)
    {
        try
        {
            var query = _context.Cars
                .Where(c => c.IsActive && c.Status == CarStatus.Available)
                .AsQueryable();

            if (branchId.HasValue)
            {
                query = query.Where(c => c.BranchId == branchId.Value);
            }

            var cars = await query.ToListAsync();
            var availableCarIds = new List<int>();

            foreach (var car in cars)
            {
                var isAvailable = await _bookingRepository.IsCarAvailableAsync(car.Id, startDate, endDate);
                if (isAvailable)
                {
                    availableCarIds.Add(car.Id);
                }
            }

            return ApiResponse<List<int>>.Success(availableCarIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available cars");
            return ApiResponse<List<int>>.Error("An error occurred while retrieving available cars", (string?)null, ApiStatusCode.InternalServerError);
        }
    }

    #endregion

    #region Helper Methods

    private List<BookingStatusStatsDto> GenerateStatusBreakdown(List<Booking> bookings)
    {
        var totalBookings = bookings.Count;
        return bookings
            .GroupBy(b => b.Status)
            .Select(g => new BookingStatusStatsDto
            {
                Status = g.Key,
                StatusName = g.Key.ToString(),
                Count = g.Count(),
                Percentage = totalBookings > 0 ? (double)g.Count() / totalBookings * 100 : 0,
                Revenue = g.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.FinalAmount)
            })
            .ToList();
    }

    private List<MonthlyBookingStatsDto> GenerateMonthlyStats(List<Booking> bookings)
    {
        return bookings
            .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
            .Select(g => new MonthlyBookingStatsDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM"),
                BookingCount = g.Count(),
                Revenue = g.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.FinalAmount),
                CompletedBookings = g.Count(b => b.Status == BookingStatus.Completed),
                CancelledBookings = g.Count(b => b.Status == BookingStatus.Canceled),
                CancellationRate = g.Any() ? (double)g.Count(b => b.Status == BookingStatus.Canceled) / g.Count() * 100 : 0
            })
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month)
            .ToList();
    }

    private Task<List<PopularCarStatsDto>> GeneratePopularCarsStats(List<Booking> bookings)
    {
        var result = bookings
            .Where(b => b.Car != null)
            .GroupBy(b => new { b.CarId, CarInfo = $"{b.Car.BrandEn} {b.Car.ModelEn} ({b.Car.Year})" })
            .Select(g => new PopularCarStatsDto
            {
                CarId = g.Key.CarId,
                CarInfo = g.Key.CarInfo,
                BookingCount = g.Count(),
                Revenue = g.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.FinalAmount),
                UtilizationRate = g.Count() * 0.1, // Simplified calculation
                AverageRating = 0 // Would be calculated from reviews
            })
            .OrderByDescending(c => c.BookingCount)
            .Take(10)
            .ToList();

        return Task.FromResult(result);
    }

    private Task<List<BranchBookingStatsDto>> GenerateBranchStats(List<Booking> bookings)
    {
        var result = bookings
            .Where(b => b.ReceivingBranch != null)
            .GroupBy(b => new { b.ReceivingBranchId, BranchName = b.ReceivingBranch.NameEn })
            .Select(g => new BranchBookingStatsDto
            {
                BranchId = g.Key.ReceivingBranchId,
                BranchName = g.Key.BranchName,
                BookingCount = g.Count(),
                Revenue = g.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.FinalAmount),
                PickupCount = g.Count(), // All bookings in this group are pickups from this branch
                ReturnCount = bookings.Count(b => b.DeliveryBranchId == g.Key.ReceivingBranchId) // Count returns to this branch
            })
            .OrderByDescending(b => b.BookingCount)
            .ToList();

        return Task.FromResult(result);
    }

    private List<PeakTimeStatsDto> GeneratePeakTimesStats(List<Booking> bookings)
    {
        var peakTimes = new List<PeakTimeStatsDto>();

        // Peak hours
        var hourlyStats = bookings
            .GroupBy(b => b.CreatedAt.Hour)
            .Select(g => new PeakTimeStatsDto
            {
                Period = "Hour",
                PeriodName = $"{g.Key}:00",
                BookingCount = g.Count(),
                Revenue = g.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.FinalAmount),
                Percentage = bookings.Any() ? (double)g.Count() / bookings.Count * 100 : 0
            })
            .OrderByDescending(h => h.BookingCount)
            .Take(5)
            .ToList();

        // Peak days of week
        var dayOfWeekStats = bookings
            .GroupBy(b => b.CreatedAt.DayOfWeek)
            .Select(g => new PeakTimeStatsDto
            {
                Period = "DayOfWeek",
                PeriodName = g.Key.ToString(),
                BookingCount = g.Count(),
                Revenue = g.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.FinalAmount),
                Percentage = bookings.Any() ? (double)g.Count() / bookings.Count * 100 : 0
            })
            .OrderByDescending(d => d.BookingCount)
            .ToList();

        peakTimes.AddRange(hourlyStats);
        peakTimes.AddRange(dayOfWeekStats);

        return peakTimes;
    }

    #endregion
}
