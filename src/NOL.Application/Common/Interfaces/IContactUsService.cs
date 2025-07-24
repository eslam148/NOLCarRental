using NOL.Application.Common.Responses;
using NOL.Application.DTOs;

namespace NOL.Application.Common.Interfaces;

public interface IContactUsService
{
    // Public endpoints (no authentication required)
    Task<ApiResponse<PublicContactUsDto>> GetActiveContactUsAsync();
    
    // Admin endpoints (authentication required)
    Task<ApiResponse<List<ContactUsDto>>> GetAllContactUsAsync();
    Task<ApiResponse<ContactUsDto>> GetContactUsByIdAsync(int id);
    Task<ApiResponse<ContactUsDto>> CreateContactUsAsync(CreateContactUsDto createContactUsDto);
    Task<ApiResponse<ContactUsDto>> UpdateContactUsAsync(int id, UpdateContactUsDto updateContactUsDto);
    Task<ApiResponse<bool>> DeleteContactUsAsync(int id);
    Task<ApiResponse<bool>> SetActiveContactUsAsync(int id);
    Task<ApiResponse<int>> GetTotalContactUsCountAsync();
}
