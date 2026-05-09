using SaaSPlatform.Core.Tenant.Models;
using SaaSPlatform.SharedKernel.Results;

namespace SaaSPlatform.Core.Tenant.Interfaces;

public interface ITenantRegistrationService
{
    Task<Result<Guid>> RegisterAsync(
        string restaurantName,
        List<Guid> cuisineIds,
        List<Guid> zoneIds,
        string? address,
        int minPrepTime,
        int maxPrepTime,
        string firstName,
        string lastName,
        string phone,
        string email,
        string password,
        string cnic,
        string? ntn,
        bool hasFoodLicense,
        string? cnicFrontUrl,
        string? cnicBackUrl);
}