using NOL.Application.Common.Responses;
using NOL.Application.DTOs;
using NOL.Domain.Enums;

namespace NOL.Application.Common.Interfaces;

public interface IExtraService
{
    Task<ApiResponse<List<ExtraDto>>> GetExtrasAsync();
    Task<ApiResponse<ExtraDto>> GetExtraByIdAsync(int id);
    Task<ApiResponse<List<ExtraDto>>> GetExtrasByTypeAsync(ExtraType type);
    Task<ApiResponse<List<ExtraDto>>> GetActiveExtrasAsync();
} 