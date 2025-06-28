using NOL.Application.Common.Interfaces;
using NOL.Application.Common.Responses;
using NOL.Application.Common.Services;
using NOL.Application.DTOs;

namespace NOL.Application.Features.Categories;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly LocalizedApiResponseService _responseService;
    private readonly ILocalizationService _localizationService;

    public CategoryService(
        ICategoryRepository categoryRepository,
        LocalizedApiResponseService responseService,
        ILocalizationService localizationService)
    {
        _categoryRepository = categoryRepository;
        _responseService = responseService;
        _localizationService = localizationService;
    }

    public async Task<ApiResponse<List<CategoryDto>>> GetCategoriesAsync()
    {
        try
        {
            var categories = await _categoryRepository.GetCategoriesOrderedAsync();
            var categoryDtos = categories.Select(MapToCategoryDto).ToList();
            return _responseService.Success(categoryDtos, "CategoriesRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<List<CategoryDto>>("InternalServerError");
        }
    }

    public async Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(int id)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
            {
                return _responseService.NotFound<CategoryDto>("ResourceNotFound");
            }

            var categoryDto = MapToCategoryDto(category);
            return _responseService.Success(categoryDto, "CategoryRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<CategoryDto>("InternalServerError");
        }
    }

    public async Task<ApiResponse<List<CategoryDto>>> GetActiveCategoriesAsync()
    {
        try
        {
            var categories = await _categoryRepository.GetActiveCategoriesAsync();
            var categoryDtos = categories.Select(MapToCategoryDto).ToList();
            return _responseService.Success(categoryDtos, "CategoriesRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<List<CategoryDto>>("InternalServerError");
        }
    }

    private CategoryDto MapToCategoryDto(Domain.Entities.Category category)
    {
        var isArabic = _localizationService.GetCurrentCulture() == "ar";

        return new CategoryDto
        {
            Id = category.Id,
            Name = isArabic ? category.NameAr : category.NameEn,
            Description = isArabic ? category.DescriptionAr : category.DescriptionEn,
            ImageUrl = category.ImageUrl,
            SortOrder = category.SortOrder
        };
    }
} 