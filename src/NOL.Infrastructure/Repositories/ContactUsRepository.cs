using Microsoft.EntityFrameworkCore;
using NOL.Application.Common.Interfaces;
using NOL.Domain.Entities;
using NOL.Infrastructure.Data;

namespace NOL.Infrastructure.Repositories;

public class ContactUsRepository : Repository<ContactUs>, IContactUsRepository
{
    private new readonly ApplicationDbContext _context;

    public ContactUsRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<ContactUs?> GetActiveContactUsAsync()
    {
        return await _context.ContactUs
            .Where(c => c.IsActive)
            .OrderByDescending(c => c.UpdatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<List<ContactUs>> GetAllContactUsAsync()
    {
        return await _context.ContactUs
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync();
    }

    public async Task<ContactUs?> GetContactUsByIdAsync(int id)
    {
        return await _context.ContactUs
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<ContactUs> CreateContactUsAsync(ContactUs contactUs)
    {
        contactUs.CreatedAt = DateTime.UtcNow;
        contactUs.UpdatedAt = DateTime.UtcNow;
        
        _context.ContactUs.Add(contactUs);
        await _context.SaveChangesAsync();
        
        return contactUs;
    }

    public async Task<ContactUs> UpdateContactUsAsync(ContactUs contactUs)
    {
        contactUs.UpdatedAt = DateTime.UtcNow;
        
        _context.ContactUs.Update(contactUs);
        await _context.SaveChangesAsync();
        
        return contactUs;
    }

    public async Task<bool> DeleteContactUsAsync(int id)
    {
        var contactUs = await GetContactUsByIdAsync(id);
        if (contactUs == null)
            return false;

        _context.ContactUs.Remove(contactUs);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> SetActiveContactUsAsync(int id)
    {
        // First, set all contact us entries to inactive
        var allContactUs = await _context.ContactUs.ToListAsync();
        foreach (var contact in allContactUs)
        {
            contact.IsActive = false;
            contact.UpdatedAt = DateTime.UtcNow;
        }

        // Then set the specified one as active
        var targetContactUs = await GetContactUsByIdAsync(id);
        if (targetContactUs == null)
            return false;

        targetContactUs.IsActive = true;
        targetContactUs.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetTotalContactUsCountAsync()
    {
        return await _context.ContactUs.CountAsync();
    }
}
