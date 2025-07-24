using NOL.Application.Common.Interfaces;
using NOL.Application.Common.Responses;
using NOL.Application.Common.Services;
using NOL.Application.DTOs;

namespace NOL.Application.Features.Branches;

public class BranchService : IBranchService
{
    private readonly IBranchRepository _branchRepository;
    private readonly LocalizedApiResponseService _responseService;
    private readonly ILocalizationService _localizationService;

    public BranchService(
        IBranchRepository branchRepository,
        LocalizedApiResponseService responseService,
        ILocalizationService localizationService)
    {
        _branchRepository = branchRepository;
        _responseService = responseService;
        _localizationService = localizationService;
    }

    public async Task<ApiResponse<List<BranchDto>>> GetBranchesAsync()
    {
        try
        {
            var branches = await _branchRepository.GetActiveBranchesAsync();
            var branchDtos = branches.Select(MapToBranchDto).ToList();
            return _responseService.Success(branchDtos, "BranchesRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<List<BranchDto>>("InternalServerError");
        }
    }

    public async Task<ApiResponse<PaginatedBranchesDto>> GetBranchesPagedAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // Limit max page size

            // Get paginated branches and total count
            var branches = await _branchRepository.GetActiveBranchesPagedAsync(page, pageSize);
            var totalCount = await _branchRepository.GetActiveBranchesCountAsync();

            // Map to DTOs
            var branchDtos = branches.Select(MapToBranchDto).ToList();

            // Calculate pagination info
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var hasPreviousPage = page > 1;
            var hasNextPage = page < totalPages;

            var paginatedResult = new PaginatedBranchesDto
            {
                Branches = branchDtos,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasPreviousPage = hasPreviousPage,
                HasNextPage = hasNextPage
            };

            return _responseService.Success(paginatedResult, "BranchesRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<PaginatedBranchesDto>("InternalServerError");
        }
    }

    public async Task<ApiResponse<BranchDto>> GetBranchByIdAsync(int id)
    {
        try
        {
            var branch = await _branchRepository.GetActiveBranchByIdAsync(id);

            if (branch == null)
            {
                return _responseService.NotFound<BranchDto>("ResourceNotFound");
            }

            var branchDto = MapToBranchDto(branch);
            return _responseService.Success(branchDto, "BranchRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<BranchDto>("InternalServerError");
        }
    }

    public async Task<ApiResponse<List<BranchDto>>> GetBranchesByCountryAsync(string country)
    {
        try
        {
            var branches = await _branchRepository.GetBranchesByCountryAsync(country);
            var branchDtos = branches.Select(MapToBranchDto).ToList();
            return _responseService.Success(branchDtos, "BranchesRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<List<BranchDto>>("InternalServerError");
        }
    }

    public async Task<ApiResponse<List<BranchDto>>> GetBranchesByCityAsync(string city)
    {
        try
        {
            var branches = await _branchRepository.GetBranchesByCityAsync(city);
            var branchDtos = branches.Select(MapToBranchDto).ToList();
            return _responseService.Success(branchDtos, "BranchesRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<List<BranchDto>>("InternalServerError");
        }
    }

    private BranchDto MapToBranchDto(Domain.Entities.Branch branch)
    {
        var isArabic = _localizationService.GetCurrentCulture() == "ar";

        return new BranchDto
        {
            Id = branch.Id,
            Name = isArabic ? branch.NameAr : branch.NameEn,
            Description = isArabic ? branch.DescriptionAr : branch.DescriptionEn,
            Address = branch.Address,
            City = branch.City,
            Country = branch.Country,
            Phone = branch.Phone,
            Email = branch.Email,
            Latitude = branch.Latitude,
            Longitude = branch.Longitude,
            WorkingHours = branch.WorkingHours
        };
    }
} 