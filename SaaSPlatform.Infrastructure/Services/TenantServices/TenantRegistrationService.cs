using SaaSPlatform.Application.Services;
using SaaSPlatform.Core.Tenant.Interfaces;
using SaaSPlatform.Core.Tenant.Models;
using SaaSPlatform.SharedKernel.Results;

namespace SaaSPlatform.Infrastructure.Services.TenantServices;

public class TenantRegistrationService : ITenantRegistrationService
{
    private readonly TenantRegistrationAppService _appService;

    public TenantRegistrationService(TenantRegistrationAppService appService) => _appService = appService;

    public async Task<Result<Guid>> RegisterAsync(
        string restaurantName, List<Guid> cuisineIds, List<Guid> zoneIds,
        string? address, int minPrepTime, int maxPrepTime,
        string firstName, string lastName, string phone, string email,
        string password, string cnic, string? ntn, bool hasFoodLicense,
        string? cnicFrontUrl, string? cnicBackUrl)
    {
        var request = new TenantRegistrationRequest
        {
            RestaurantName = restaurantName,
            CuisineIds = cuisineIds,
            ZoneIds = zoneIds,
            Address = address,
            MinPreparingTime = minPrepTime,
            MaxPreparingTime = maxPrepTime,
            FirstName = firstName,
            LastName = lastName,
            Phone = phone,
            Email = email,
            Password = password,
            CnicNumber = cnic,
            NtnNumber = ntn,
            HasFoodLicense = hasFoodLicense,
            CnicFrontImageUrl = cnicFrontUrl,
            CnicBackImageUrl = cnicBackUrl
        };

        return await _appService.RegisterAsync(request);
    }
}