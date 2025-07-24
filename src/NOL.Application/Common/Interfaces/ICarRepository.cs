using NOL.Domain.Entities;
using NOL.Domain.Enums;

namespace NOL.Application.Common.Interfaces;

public interface ICarRepository : IRepository<Car>
{
    // Read operations
    Task<IEnumerable<Car>> GetCarsAsync(string? sortByCost = null, int page = 1, int pageSize = 10, string? brand = null);
    Task<IEnumerable<Car>> GetAvailableCarsAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Car>> GetCarsByCategoryAsync(int categoryId);
    Task<IEnumerable<Car>> GetCarsByBranchAsync(int branchId);
    Task<Car?> GetCarWithIncludesAsync(int id);
    Task<IEnumerable<Car>> GetCarsWithIncludesAsync();

    // Search operations
    Task<IEnumerable<Car>> SearchCarsAsync(string searchTerm, int page = 1, int pageSize = 10);
    Task<bool> IsPlateNumberUniqueAsync(string plateNumber, int? excludeCarId = null);

    // Rate management operations
    Task<bool> UpdateCarRatesAsync(int carId, decimal dailyRate, decimal weeklyRate, decimal monthlyRate);
    Task<IEnumerable<Car>> GetCarsWithRatesAsync(int page = 1, int pageSize = 10);
    Task<bool> BulkUpdateRatesAsync(Dictionary<int, (decimal daily, decimal weekly, decimal monthly)> rateUpdates);

    // Status management
    Task<bool> UpdateCarStatusAsync(int carId, CarStatus status);
    Task<IEnumerable<Car>> GetCarsByStatusAsync(CarStatus status);

    // Validation operations
    Task<bool> ExistsAsync(int id);
    Task<bool> IsCategoryValidAsync(int categoryId);
    Task<bool> IsBranchValidAsync(int branchId);
}