using NOL.Domain.Entities;

namespace NOL.Application.Common.Interfaces;

public interface IBranchRepository : IRepository<Branch>
{
    Task<IEnumerable<Branch>> GetActiveBranchesAsync();
    Task<IEnumerable<Branch>> GetActiveBranchesPagedAsync(int page, int pageSize);
    Task<int> GetActiveBranchesCountAsync();
    Task<Branch?> GetActiveBranchByIdAsync(int id);
    Task<IEnumerable<Branch>> GetBranchesByCountryAsync(string country);
    Task<IEnumerable<Branch>> GetBranchesByCityAsync(string city);
    Task<IEnumerable<Branch>> GetActiveBranchesNearbyAsync(decimal latitude, decimal longitude, double radiusKm);
    Task<int> GetActiveBranchesNearbyCountAsync(decimal latitude, decimal longitude, double radiusKm);
}