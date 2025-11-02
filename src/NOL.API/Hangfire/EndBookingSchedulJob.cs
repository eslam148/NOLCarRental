using Microsoft.EntityFrameworkCore;
using NOL.Domain.Enums;
using NOL.Infrastructure.Data;

namespace NOL.Application.Hangfire;

public class EndBookingSchedulJob
{
    private readonly ApplicationDbContext  _context;
    public EndBookingSchedulJob(ApplicationDbContext  context)
    {
        _context = context;
    }

    public async Task Execute()
    {
        try
        {
           // await _context.Bookings.Where(x => x.EndDate != null && x.EndDate < DateTime.UtcNow && x.Status <BookingStatus.InProgress )
           //      .ExecuteUpdateAsync(x=>x.SetProperty((booking => booking.Status),BookingStatus.Closed));
        }
        catch (Exception ex)
        {
            
        }
    }
}