using NOL.Application.Common.Interfaces;
using NOL.Application.Common.Responses;
using NOL.Application.Common.Services;
using NOL.Application.DTOs;
using NOL.Domain.Entities;

namespace NOL.Application.Features.ContactUs;

public class ContactUsService : IContactUsService
{
    private readonly IContactUsRepository _contactUsRepository;
    private readonly LocalizedApiResponseService _responseService;

    public ContactUsService(
        IContactUsRepository contactUsRepository,
        LocalizedApiResponseService responseService)
    {
        _contactUsRepository = contactUsRepository;
        _responseService = responseService;
    }

    public async Task<ApiResponse<PublicContactUsDto>> GetActiveContactUsAsync()
    {
        try
        {
            var activeContactUs = await _contactUsRepository.GetActiveContactUsAsync();
            
            if (activeContactUs == null)
            {
                // Return empty contact info if none is active
                var emptyContactUs = new PublicContactUsDto();
                return _responseService.Success(emptyContactUs, "NoActiveContactUsFound");
            }

            var publicContactUsDto = new PublicContactUsDto
            {
                Email = activeContactUs.Email,
                Phone = activeContactUs.Phone,
                WhatsApp = activeContactUs.WhatsApp,
                Facebook = activeContactUs.Facebook,
                Instagram = activeContactUs.Instagram,
                X = activeContactUs.X,
                TikTok = activeContactUs.TikTok
            };

            return _responseService.Success(publicContactUsDto, "ActiveContactUsRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<PublicContactUsDto>("InternalServerError");
        }
    }

    public async Task<ApiResponse<List<ContactUsDto>>> GetAllContactUsAsync()
    {
        try
        {
            var contactUsList = await _contactUsRepository.GetAllContactUsAsync();
            
            var contactUsDtos = contactUsList.Select(c => new ContactUsDto
            {
                Id = c.Id,
                Email = c.Email,
                Phone = c.Phone,
                WhatsApp = c.WhatsApp,
                Facebook = c.Facebook,
                Instagram = c.Instagram,
                X = c.X,
                TikTok = c.TikTok,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                IsActive = c.IsActive
            }).ToList();

            return _responseService.Success(contactUsDtos, "ContactUsListRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<List<ContactUsDto>>("InternalServerError");
        }
    }

    public async Task<ApiResponse<ContactUsDto>> GetContactUsByIdAsync(int id)
    {
        try
        {
            var contactUs = await _contactUsRepository.GetContactUsByIdAsync(id);
            
            if (contactUs == null)
            {
                return _responseService.NotFound<ContactUsDto>("ContactUsNotFound");
            }

            var contactUsDto = new ContactUsDto
            {
                Id = contactUs.Id,
                Email = contactUs.Email,
                Phone = contactUs.Phone,
                WhatsApp = contactUs.WhatsApp,
                Facebook = contactUs.Facebook,
                Instagram = contactUs.Instagram,
                X = contactUs.X,
                TikTok = contactUs.TikTok,
                CreatedAt = contactUs.CreatedAt,
                UpdatedAt = contactUs.UpdatedAt,
                IsActive = contactUs.IsActive
            };

            return _responseService.Success(contactUsDto, "ContactUsRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<ContactUsDto>("InternalServerError");
        }
    }

    public async Task<ApiResponse<ContactUsDto>> CreateContactUsAsync(CreateContactUsDto createContactUsDto)
    {
        try
        {
            var contactUs = new Domain.Entities.ContactUs
            {
                Email = createContactUsDto.Email,
                Phone = createContactUsDto.Phone,
                WhatsApp = createContactUsDto.WhatsApp,
                Facebook = createContactUsDto.Facebook,
                Instagram = createContactUsDto.Instagram,
                X = createContactUsDto.X,
                TikTok = createContactUsDto.TikTok,
                IsActive = true // New contact us entries are active by default
            };

            // If this is the first contact us entry, make it active
            // Otherwise, set all others to inactive and make this one active
            var existingCount = await _contactUsRepository.GetTotalContactUsCountAsync();
            if (existingCount > 0)
            {
                // Set all existing entries to inactive
                var allContactUs = await _contactUsRepository.GetAllContactUsAsync();
                foreach (var existing in allContactUs)
                {
                    existing.IsActive = false;
                    await _contactUsRepository.UpdateContactUsAsync(existing);
                }
            }

            var createdContactUs = await _contactUsRepository.CreateContactUsAsync(contactUs);

            var contactUsDto = new ContactUsDto
            {
                Id = createdContactUs.Id,
                Email = createdContactUs.Email,
                Phone = createdContactUs.Phone,
                WhatsApp = createdContactUs.WhatsApp,
                Facebook = createdContactUs.Facebook,
                Instagram = createdContactUs.Instagram,
                X = createdContactUs.X,
                TikTok = createdContactUs.TikTok,
                CreatedAt = createdContactUs.CreatedAt,
                UpdatedAt = createdContactUs.UpdatedAt,
                IsActive = createdContactUs.IsActive
            };

            return _responseService.Success(contactUsDto, "ContactUsCreated");
        }
        catch (Exception)
        {
            return _responseService.Error<ContactUsDto>("InternalServerError");
        }
    }

    public async Task<ApiResponse<ContactUsDto>> UpdateContactUsAsync(int id, UpdateContactUsDto updateContactUsDto)
    {
        try
        {
            var existingContactUs = await _contactUsRepository.GetContactUsByIdAsync(id);
            
            if (existingContactUs == null)
            {
                return _responseService.NotFound<ContactUsDto>("ContactUsNotFound");
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(updateContactUsDto.Email))
                existingContactUs.Email = updateContactUsDto.Email;
            
            if (!string.IsNullOrEmpty(updateContactUsDto.Phone))
                existingContactUs.Phone = updateContactUsDto.Phone;
            
            if (!string.IsNullOrEmpty(updateContactUsDto.WhatsApp))
                existingContactUs.WhatsApp = updateContactUsDto.WhatsApp;
            
            if (!string.IsNullOrEmpty(updateContactUsDto.Facebook))
                existingContactUs.Facebook = updateContactUsDto.Facebook;
            
            if (!string.IsNullOrEmpty(updateContactUsDto.Instagram))
                existingContactUs.Instagram = updateContactUsDto.Instagram;
            
            if (!string.IsNullOrEmpty(updateContactUsDto.X))
                existingContactUs.X = updateContactUsDto.X;
            
            if (!string.IsNullOrEmpty(updateContactUsDto.TikTok))
                existingContactUs.TikTok = updateContactUsDto.TikTok;
            
            if (updateContactUsDto.IsActive.HasValue)
                existingContactUs.IsActive = updateContactUsDto.IsActive.Value;

            var updatedContactUs = await _contactUsRepository.UpdateContactUsAsync(existingContactUs);

            var contactUsDto = new ContactUsDto
            {
                Id = updatedContactUs.Id,
                Email = updatedContactUs.Email,
                Phone = updatedContactUs.Phone,
                WhatsApp = updatedContactUs.WhatsApp,
                Facebook = updatedContactUs.Facebook,
                Instagram = updatedContactUs.Instagram,
                X = updatedContactUs.X,
                TikTok = updatedContactUs.TikTok,
                CreatedAt = updatedContactUs.CreatedAt,
                UpdatedAt = updatedContactUs.UpdatedAt,
                IsActive = updatedContactUs.IsActive
            };

            return _responseService.Success(contactUsDto, "ContactUsUpdated");
        }
        catch (Exception)
        {
            return _responseService.Error<ContactUsDto>("InternalServerError");
        }
    }

    public async Task<ApiResponse<bool>> DeleteContactUsAsync(int id)
    {
        try
        {
            var contactUs = await _contactUsRepository.GetContactUsByIdAsync(id);
            
            if (contactUs == null)
            {
                return _responseService.NotFound<bool>("ContactUsNotFound");
            }

            var deleted = await _contactUsRepository.DeleteContactUsAsync(id);
            
            if (deleted)
            {
                return _responseService.Success(true, "ContactUsDeleted");
            }
            else
            {
                return _responseService.Error<bool>("ContactUsDeletionFailed");
            }
        }
        catch (Exception)
        {
            return _responseService.Error<bool>("InternalServerError");
        }
    }

    public async Task<ApiResponse<bool>> SetActiveContactUsAsync(int id)
    {
        try
        {
            var contactUs = await _contactUsRepository.GetContactUsByIdAsync(id);
            
            if (contactUs == null)
            {
                return _responseService.NotFound<bool>("ContactUsNotFound");
            }

            var result = await _contactUsRepository.SetActiveContactUsAsync(id);
            
            if (result)
            {
                return _responseService.Success(true, "ContactUsSetAsActive");
            }
            else
            {
                return _responseService.Error<bool>("ContactUsActivationFailed");
            }
        }
        catch (Exception)
        {
            return _responseService.Error<bool>("InternalServerError");
        }
    }

    public async Task<ApiResponse<int>> GetTotalContactUsCountAsync()
    {
        try
        {
            var count = await _contactUsRepository.GetTotalContactUsCountAsync();
            return _responseService.Success(count, "ContactUsCountRetrieved");
        }
        catch (Exception)
        {
            return _responseService.Error<int>("InternalServerError");
        }
    }
}
