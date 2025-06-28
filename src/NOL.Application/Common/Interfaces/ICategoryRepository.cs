using NOL.Domain.Entities;

namespace NOL.Application.Common.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetCategoriesOrderedAsync();
    Task<IEnumerable<Category>> GetActiveCategoriesAsync();
} 