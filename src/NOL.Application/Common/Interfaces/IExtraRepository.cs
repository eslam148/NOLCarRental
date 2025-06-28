using NOL.Domain.Entities;
using NOL.Domain.Enums;

namespace NOL.Application.Common.Interfaces;

public interface IExtraRepository : IRepository<ExtraTypePrice>
{
    Task<IEnumerable<ExtraTypePrice>> GetExtrasByTypeAsync(ExtraType type);
    Task<IEnumerable<ExtraTypePrice>> GetActiveExtrasAsync();
} 