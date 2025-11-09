using NOL.Application.Common.Interfaces;
using NOL.Application.Common.Responses;
using NOL.Application.Common.Services;
using NOL.Application.DTOs;
using NOL.Domain.Enums;

namespace NOL.Application.Features.Extras;

public class ExtraService : IExtraService
{
    private readonly IExtraRepository _extraRepository;
    private readonly LocalizedApiResponseService _responseService;
    private readonly ILocalizationService _localizationService;

    public ExtraService(
        IExtraRepository extraRepository,
        LocalizedApiResponseService responseService,
        ILocalizationService localizationService)
    {
        _extraRepository = extraRepository;
        _responseService = responseService;
        _localizationService = localizationService;
    }

    public async Task<ApiResponse<List<ExtraDto>>> GetExtrasAsync()
    {
      
            var extras = await _extraRepository.GetAllAsync();
            var extraDtos = extras.Select(MapToExtraDto).ToList();
            return _responseService.Success(extraDtos, ResponseCode.ExtrasRetrieved);
        
    }

    public async Task<ApiResponse<ExtraDto>> GetExtraByIdAsync(int id)
    {
       
            var extra = await _extraRepository.GetByIdAsync(id);

            if (extra == null)
            {
                return _responseService.NotFound<ExtraDto>(ResponseCode.ExtraServiceNotFound);
            }

            var extraDto = MapToExtraDto(extra);
            return _responseService.Success(extraDto, ResponseCode.ExtraRetrieved);
       
    }

    public async Task<ApiResponse<List<ExtraDto>>> GetExtrasByTypeAsync(ExtraType type)
    {
         
            var extras = await _extraRepository.GetExtrasByTypeAsync(type);
            var extraDtos = extras.Select(MapToExtraDto).ToList();
            return _responseService.Success(extraDtos, ResponseCode.ExtraRetrieved);
        
    }

    public async Task<ApiResponse<List<ExtraDto>>> GetActiveExtrasAsync()
    {
        
            var extras = await _extraRepository.GetActiveExtrasAsync();
            var extraDtos = extras.Select(MapToExtraDto).ToList();
            return _responseService.Success(extraDtos, ResponseCode.ExtraRetrieved);
        
    }

    private ExtraDto MapToExtraDto(Domain.Entities.ExtraTypePrice extra)
    {
        var isArabic = _localizationService.GetCurrentCulture() == "ar";

        return new ExtraDto
        {
            Id = extra.Id,
            ExtraType = extra.ExtraType,
            Name = isArabic ? extra.NameAr : extra.NameEn,
            Description = isArabic ? extra.DescriptionAr : extra.DescriptionEn,
            DailyPrice = extra.DailyPrice,
            WeeklyPrice = extra.WeeklyPrice,
            MonthlyPrice = extra.MonthlyPrice
        };
    }
} 