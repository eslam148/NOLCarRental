using NOL.Domain.Entities;

namespace NOL.Application.Common.Interfaces;

public interface IContactUsRepository : IRepository<ContactUs>
{
    Task<ContactUs?> GetActiveContactUsAsync();
    Task<List<ContactUs>> GetAllContactUsAsync();
    Task<ContactUs?> GetContactUsByIdAsync(int id);
    Task<ContactUs> CreateContactUsAsync(ContactUs contactUs);
    Task<ContactUs> UpdateContactUsAsync(ContactUs contactUs);
    Task<bool> DeleteContactUsAsync(int id);
    Task<bool> SetActiveContactUsAsync(int id);
    Task<int> GetTotalContactUsCountAsync();
}
