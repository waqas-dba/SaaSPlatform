// SaaSPlatform.Infrastructure/Services/TenantServices/Service/TenantRegistrationService.cs
using Microsoft.EntityFrameworkCore;
using SaaSPlatform.Core.Billing.Entities;
using SaaSPlatform.Core.Billing.Enums;
using SaaSPlatform.Core.IAM.Entities;
using SaaSPlatform.Core.IAM.Interfaces;
using SaaSPlatform.Core.Tenant.Entities;
using SaaSPlatform.Core.Tenant.Enums;
using SaaSPlatform.Core.Tenant.Interfaces;
using SaaSPlatform.Infrastructure.Persistence;
using TenantEntity = SaaSPlatform.Core.Tenant.Entities.Tenant;

namespace SaaSPlatform.Infrastructure.Services.TenantServices.Service;

public class TenantRegistrationService : ITenantRegistrationService
{
    private readonly SaaSPlatformDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public TenantRegistrationService(
        SaaSPlatformDbContext db,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<(Guid tenantId, string message)> RegisterAsync(
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
        string? cnicBackUrl)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            var slug = GenerateSlug(restaurantName);

            // 1. Tenant – inactive, pending approval
            var tenant = new TenantEntity
            {
                Name = restaurantName,
                Slug = slug,
                IsActive = false,
                RegistrationStatus = RegistrationStatus.Pending
            };
            _db.Tenants.Add(tenant);
            await _db.SaveChangesAsync();

            // 2. Store
            var store = new Store
            {
                TenantId = tenant.Id,
                Name = restaurantName,
                Slug = slug,
                Address = address,
                MinPreparingTime = minPrepTime,
                MaxPreparingTime = maxPrepTime,
                IsOnline = false
            };
            _db.Stores.Add(store);
            await _db.SaveChangesAsync();

            // 3. Cuisines & Zones
            foreach (var cuisineId in cuisineIds)
                _db.StoreCuisines.Add(new StoreCuisine { StoreId = store.Id, CuisineId = cuisineId });
            foreach (var zoneId in zoneIds)
                _db.StoreDeliveryZones.Add(new StoreDeliveryZone { StoreId = store.Id, ZoneId = zoneId });

            // 4. Owner User
            var user = new User
            {
                Name = $"{firstName} {lastName}",
                Phone = phone,
                Email = email,
                PasswordHash = _passwordHasher.Hash(password),
                IsActive = true       // user is active, but tenant is not
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // 5. Tenant-specific Owner role with all permissions
            var ownerRole = new Role
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Name = "Owner",
                Description = "Tenant owner – full access",
                IsSystem = false
            };
            _db.Roles.Add(ownerRole);
            await _db.SaveChangesAsync();

            var allPermissions = await _db.Permissions.ToListAsync();
            foreach (var permission in allPermissions)
                _db.RolePermissions.Add(new RolePermission { RoleId = ownerRole.Id, PermissionId = permission.Id });

            // 6. Link user to tenant as owner
            _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = ownerRole.Id, TenantId = tenant.Id });
            _db.TenantUsers.Add(new TenantUser { TenantId = tenant.Id, UserId = user.Id, IsOwner = true, IsActive = true });

            // 7. Legal info
            _db.TenantLegalInfos.Add(new TenantLegalInfo
            {
                TenantId = tenant.Id,
                CnicNumber = cnic,
                NtnNumber = ntn,
                HasFoodLicense = hasFoodLicense,
                CnicFrontImageUrl = cnicFrontUrl,
                CnicBackImageUrl = cnicBackUrl
            });

            // 8. Subscription – Starter plan, trialing
            var starterPlan = await _db.Plans.FirstAsync(p => p.Name == "Starter");
            _db.Subscriptions.Add(new Subscription
            {
                TenantId = tenant.Id,
                PlanId = starterPlan.Id,
                Status = SubscriptionStatus.Trialing,
                StartDate = DateTime.UtcNow,
                TrialEndsAt = DateTime.UtcNow.AddDays(14),
                NextBillingDate = DateTime.UtcNow.AddDays(14),
                PriceSnapshot = 0
            });

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            // No token generation – owner must wait for approval
            return (tenant.Id, "Registration submitted successfully. Awaiting approval.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static string GenerateSlug(string name)
    {
        return name.ToLowerInvariant()
                   .Replace(" ", "-")
                   .Replace("'", "")
                   .Replace("\"", "");
    }
}