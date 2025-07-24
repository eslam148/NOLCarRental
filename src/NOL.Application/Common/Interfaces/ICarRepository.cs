using NOL.Domain.Entities;
using NOL.Domain.Enums;

namespace NOL.Application.Common.Interfaces;

public interface ICarRepository : IRepository<Car>
{
    Task<IEnumerable<Car>> GetCarsAsync(string? sortByCost = null, int page = 1, int pageSize = 10, string? brand = null);
    Task<IEnumerable<Car>> GetAvailableCarsAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Car>> GetCarsByCategoryAsync(int categoryId);
    Task<IEnumerable<Car>> GetCarsByBranchAsync(int branchId);
    Task<Car?> GetCarWithIncludesAsync(int id);
    Task<IEnumerable<Car>> GetCarsWithIncludesAsync();
} 