using Microsoft.EntityFrameworkCore;
using NOL.Application.Common.Interfaces;
using NOL.Domain.Entities;
using NOL.Domain.Enums;
using NOL.Infrastructure.Data;

namespace NOL.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly ApplicationDbContext _context;

    public BookingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Booking>> GetUserBookingsAsync(string userId)
    {
        return await _context.Bookings
            .Include(b => b.Car)
                .ThenInclude(c => c.Category)
            .Include(b => b.ReceivingBranch)
            .Include(b => b.DeliveryBranch)
            .Include(b => b.BookingExtras)
                .ThenInclude(be => be.ExtraTypePrice)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Booking>> GetUserBookingsByStatusAsync(string userId, BookingStatus status)
    {
        return await _context.Bookings
            .Include(b => b.Car)
                .ThenInclude(c => c.Category)
            .Include(b => b.ReceivingBranch)
            .Include(b => b.DeliveryBranch)
            .Include(b => b.BookingExtras)
                .ThenInclude(be => be.ExtraTypePrice)
            .Where(b => b.UserId == userId && b.Status == status)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<Booking?> GetUserBookingByIdAsync(int bookingId, string userId)
    {
        return await _context.Bookings
            .Include(b => b.Car)
                .ThenInclude(c => c.Category)
            .Include(b => b.User)
            .Include(b => b.ReceivingBranch)
            .Include(b => b.DeliveryBranch)
            .Include(b => b.BookingExtras)
                .ThenInclude(be => be.ExtraTypePrice)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);
    }

    public async Task<Booking?> GetBookingByIdAsync(int id)
    {
        return await _context.Bookings
            .Include(b => b.Car)
                .ThenInclude(c => c.Category)
            .Include(b => b.User)
            .Include(b => b.ReceivingBranch)
            .Include(b => b.DeliveryBranch)
            .Include(b => b.BookingExtras)
                .ThenInclude(be => be.ExtraTypePrice)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Booking> CreateBookingAsync(Booking booking)
    {
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
        return booking;
    }

    public async Task<Booking> UpdateBookingAsync(Booking booking)
    {
        _context.Bookings.Update(booking);
        await _context.SaveChangesAsync();
        return booking;
    }

    public async Task<bool> IsCarAvailableAsync(int carId, DateTime startDate, DateTime endDate, int? excludeBookingId = null)
    {
        var conflictingBookings = await _context.Bookings
            .Where(b => b.CarId == carId &&
                       b.Status != BookingStatus.Canceled &&
                       ((startDate >= b.StartDate && startDate < b.EndDate) ||
                        (endDate > b.StartDate && endDate <= b.EndDate) ||
                        (startDate <= b.StartDate && endDate >= b.EndDate)))
            .Where(b => excludeBookingId == null || b.Id != excludeBookingId)
            .AnyAsync();

        return !conflictingBookings;
    }

    public async Task<bool> IsBranchAvailableAsync(int branchId)
    {
        var branch = await _context.Branches.FindAsync(branchId);
        return branch != null && branch.IsActive;
    }

    public async Task<Car?> GetCarByIdAsync(int carId)
    {
        return await _context.Cars
            .Include(c => c.Category)
            .FirstOrDefaultAsync(c => c.Id == carId);
    }

    public async Task<List<ExtraTypePrice>> GetExtraTypePricesByIdsAsync(List<int> ids)
    {
        return await _context.ExtraTypePrices
            .Where(etp => ids.Contains(etp.Id))
            .ToListAsync();
    }

    public async Task<List<ExtraTypePrice>> GetExtraTypePricesAsync(List<int> extraTypePriceIds)
    {
        return await _context.ExtraTypePrices
            .Where(etp => extraTypePriceIds.Contains(etp.Id))
            .ToListAsync();
    }

    public async Task AddBookingExtrasAsync(List<BookingExtra> bookingExtras)
    {
        _context.BookingExtras.AddRange(bookingExtras);
        await _context.SaveChangesAsync();
    }
} 