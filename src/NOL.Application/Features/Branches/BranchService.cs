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
          var branches = await _branchRepository.GetActiveBranchesAsync();
            var branchDtos = branches.Select(MapToBranchDto).ToList();
            return _responseService.Success(branchDtos, ResponseCode.BranchesRetrieved);
         
    }

    public async Task<ApiResponse<PaginatedBranchesDto>> GetBranchesPagedAsync(int page = 1, int pageSize = 10)
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

            return _responseService.Success(paginatedResult, ResponseCode.BranchesRetrieved);
       
    }

    public async Task<ApiResponse<BranchDto>> GetBranchByIdAsync(int id)
    {
        
            var branch = await _branchRepository.GetActiveBranchByIdAsync(id);

            if (branch == null)
            {
                return _responseService.NotFound<BranchDto>(ResponseCode.BranchNotFound);
            }

            var branchDto = MapToBranchDto(branch);
            return _responseService.Success(branchDto, ResponseCode.BranchesRetrieved);
      
    }

    public async Task<ApiResponse<List<BranchDto>>> GetBranchesByCountryAsync(string country)
    {
         
            var branches = await _branchRepository.GetBranchesByCountryAsync(country);
            var branchDtos = branches.Select(MapToBranchDto).ToList();
            return _responseService.Success(branchDtos, ResponseCode.BranchesRetrieved);
       
    }

    public async Task<ApiResponse<List<BranchDto>>> GetBranchesByCityAsync(string city)
    {
         
            var branches = await _branchRepository.GetBranchesByCityAsync(city);
            var branchDtos = branches.Select(MapToBranchDto).ToList();
            return _responseService.Success(branchDtos, ResponseCode.BranchesRetrieved);
       
    }

    public async Task<ApiResponse<PaginatedBranchesDto>> GetBranchesNearbyAsync(decimal latitude, decimal longitude, double radiusKm = 50, int page = 1, int pageSize = 10)
    {
         
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // Limit max page size

            // Validate coordinates
            if (latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180)
            {
                return _responseService.Error<PaginatedBranchesDto>(ResponseCode.InvalidCoordinates);
            }

            // Validate radius
            if (radiusKm <= 0 || radiusKm > 1000) // Max 1000km radius
            {
                radiusKm = 50; // Default to 50km
            }

            // Get nearby branches and total count
            var allNearbyBranches = await _branchRepository.GetActiveBranchesNearbyAsync(latitude, longitude, radiusKm);
            var totalCount = allNearbyBranches.Count();

            // Apply pagination
            var paginatedBranches = allNearbyBranches
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Map to DTOs
            var branchDtos = paginatedBranches.Select(MapToBranchDto).ToList();

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

            return _responseService.Success(paginatedResult, ResponseCode.NearbyBranchesRetrieved);
         
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