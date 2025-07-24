using NOL.Application.Common.Responses;
using NOL.Application.DTOs;

namespace NOL.Application.Common.Interfaces;

public interface IBranchService
{
    Task<ApiResponse<List<BranchDto>>> GetBranchesAsync();
    Task<ApiResponse<PaginatedBranchesDto>> GetBranchesPagedAsync(int page = 1, int pageSize = 10);
    Task<ApiResponse<BranchDto>> GetBranchByIdAsync(int id);
    Task<ApiResponse<List<BranchDto>>> GetBranchesByCountryAsync(string country);
    Task<ApiResponse<List<BranchDto>>> GetBranchesByCityAsync(string city);
}