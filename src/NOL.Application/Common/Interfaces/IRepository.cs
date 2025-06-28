using System.Linq.Expressions;

namespace NOL.Application.Common.Interfaces;

public interface IRepository<T> where T : class
{
    // Basic CRUD operations
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    
    // Pagination
    Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize);
    Task<IEnumerable<T>> GetPagedAsync(Expression<Func<T, bool>> predicate, int page, int pageSize);
    
    // Count operations
    Task<int> CountAsync();
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);
    
    // Add/Update/Delete operations
    Task<T> AddAsync(T entity);
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task DeleteAsync(int id);
    Task DeleteRangeAsync(IEnumerable<T> entities);
    
    // Save changes
    Task<int> SaveChangesAsync();
} 