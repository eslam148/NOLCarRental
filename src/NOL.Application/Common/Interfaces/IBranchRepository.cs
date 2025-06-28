using NOL.Domain.Entities;

namespace NOL.Application.Common.Interfaces;

public interface IBranchRepository : IRepository<Branch>
{
    Task<IEnumerable<Branch>> GetActiveBranchesAsync();
    Task<Branch?> GetActiveBranchByIdAsync(int id);
    Task<IEnumerable<Branch>> GetBranchesByCountryAsync(string country);
    Task<IEnumerable<Branch>> GetBranchesByCityAsync(string city);
} 