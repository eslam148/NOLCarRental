using Microsoft.EntityFrameworkCore;
using NOL.Application.Common.Interfaces;
using NOL.Domain.Entities;
using NOL.Domain.Enums;
using NOL.Infrastructure.Data;

namespace NOL.Infrastructure.Repositories;

public class LoyaltyPointRepository : ILoyaltyPointRepository
{
    private readonly ApplicationDbContext _context;

    public LoyaltyPointRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<LoyaltyPointTransaction>> GetUserTransactionsAsync(string userId, int page = 1, int pageSize = 10)
    {
        return await _context.LoyaltyPointTransactions
            .Include(lpt => lpt.Booking)
            .Where(lpt => lpt.UserId == userId)
            .OrderByDescending(lpt => lpt.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<LoyaltyPointTransaction> CreateTransactionAsync(LoyaltyPointTransaction transaction)
    {
        _context.LoyaltyPointTransactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<int> GetUserAvailablePointsAsync(string userId)
    {
        var earnedPoints = await _context.LoyaltyPointTransactions
            .Where(lpt => lpt.UserId == userId && 
                         (lpt.TransactionType == LoyaltyPointTransactionType.Earned || 
                          lpt.TransactionType == LoyaltyPointTransactionType.Bonus ||
                          lpt.TransactionType == LoyaltyPointTransactionType.Refund) &&
                         !lpt.IsExpired)
            .SumAsync(lpt => lpt.Points);

        var usedPoints = await _context.LoyaltyPointTransactions
            .Where(lpt => lpt.UserId == userId && 
                         (lpt.TransactionType == LoyaltyPointTransactionType.Redeemed ||
                          lpt.TransactionType == LoyaltyPointTransactionType.Expired ||
                          lpt.TransactionType == LoyaltyPointTransactionType.Adjustment))
            .SumAsync(lpt => Math.Abs(lpt.Points));

        return Math.Max(0, earnedPoints - usedPoints);
    }

    public async Task<int> GetUserTotalPointsAsync(string userId)
    {
        return await _context.LoyaltyPointTransactions
            .Where(lpt => lpt.UserId == userId && 
                         (lpt.TransactionType == LoyaltyPointTransactionType.Earned ||
                          lpt.TransactionType == LoyaltyPointTransactionType.Bonus ||
                          lpt.TransactionType == LoyaltyPointTransactionType.Refund))
            .SumAsync(lpt => lpt.Points);
    }

    public async Task<List<LoyaltyPointTransaction>> GetExpiringPointsAsync(string userId, DateTime beforeDate)
    {
        return await _context.LoyaltyPointTransactions
            .Where(lpt => lpt.UserId == userId &&
                         lpt.ExpiryDate.HasValue &&
                         lpt.ExpiryDate.Value <= beforeDate &&
                         !lpt.IsExpired &&
                         (lpt.TransactionType == LoyaltyPointTransactionType.Earned ||
                          lpt.TransactionType == LoyaltyPointTransactionType.Bonus))
            .ToListAsync();
    }

    public async Task<LoyaltyPointTransaction?> GetTransactionByIdAsync(int id)
    {
        return await _context.LoyaltyPointTransactions
            .Include(lpt => lpt.Booking)
            .Include(lpt => lpt.User)
            .FirstOrDefaultAsync(lpt => lpt.Id == id);
    }

    public async Task ExpirePointsAsync(List<int> transactionIds)
    {
        var transactions = await _context.LoyaltyPointTransactions
            .Where(lpt => transactionIds.Contains(lpt.Id))
            .ToListAsync();

        foreach (var transaction in transactions)
        {
            transaction.IsExpired = true;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasUserEarnedPointsForBookingAsync(string userId, int bookingId)
    {
        return await _context.LoyaltyPointTransactions
            .AnyAsync(lpt => lpt.UserId == userId && 
                            lpt.BookingId == bookingId &&
                            lpt.TransactionType == LoyaltyPointTransactionType.Earned);
    }

    public async Task UpdateUserLoyaltyPointsAsync(string userId, int availablePoints, int totalPoints, int lifetimeEarned, int lifetimeRedeemed)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.AvailableLoyaltyPoints = availablePoints;
            user.TotalLoyaltyPoints = totalPoints;
            user.LifetimePointsEarned = lifetimeEarned;
            user.LifetimePointsRedeemed = lifetimeRedeemed;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
} 