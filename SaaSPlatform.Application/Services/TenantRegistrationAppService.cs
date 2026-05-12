using SaaSPlatform.Core.Billing.Entities;
using SaaSPlatform.Core.Billing.Enums;
using SaaSPlatform.Core.Billing.Interfaces;
using AuthCoreKit.IAM.Entities;          // User, Role, UserRole, RolePermission
using AuthCoreKit.IAM.Interfaces;        // IUserRepository, IRoleRepository, IPasswordHasher
using SaaSPlatform.Core.Tenant.Entities;
using SaaSPlatform.Core.Tenant.Enums;
using SaaSPlatform.Core.Tenant.Interfaces;
using SaaSPlatform.Core.Tenant.Models;
using SaaSPlatform.SharedKernel.Interfaces;
using SaaSPlatform.SharedKernel.Results;

namespace SaaSPlatform.Application.Services;

public class TenantRegistrationAppService
{
    private readonly ITenantAccountRepository _tenantRepo;
    private readonly IStoreRepository _storeRepo;
    private readonly IUserRepository _userRepo;
    private readonly IRoleRepository _roleRepo;
    private readonly ITenantLegalInfoRepository _legalRepo;
    private readonly ISubscriptionRepository _subscriptionRepo;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISlugGenerator _slugGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public TenantRegistrationAppService(
        ITenantAccountRepository tenantRepo,
        IStoreRepository storeRepo,
        IUserRepository userRepo,
        IRoleRepository roleRepo,
        ITenantLegalInfoRepository legalRepo,
        ISubscriptionRepository subscriptionRepo,
        IPasswordHasher passwordHasher,
        ISlugGenerator slugGenerator,
        IUnitOfWork unitOfWork)
    {
        _tenantRepo = tenantRepo;
        _storeRepo = storeRepo;
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _legalRepo = legalRepo;
        _subscriptionRepo = subscriptionRepo;
        _passwordHasher = passwordHasher;
        _slugGenerator = slugGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> RegisterAsync(TenantRegistrationRequest request)
    {
        // 1. Tenant name uniqueness (already implemented)
        if (await _tenantRepo.ExistsByNameAsync(request.RestaurantName))
            return Result<Guid>.Failure("A tenant with this name already exists.");

        // 2. Global phone uniqueness – use ExistsByPhoneAsync with tenantId: null
        if (await _userRepo.ExistsByPhoneAsync(request.Phone, tenantId: null))
            return Result<Guid>.Failure("A user with this phone number already exists. One owner can only have one account.");

        // 3. Global email uniqueness (optional)
        if (!string.IsNullOrWhiteSpace(request.Email) &&
            await _userRepo.ExistsByEmailAsync(request.Email, tenantId: null))
            return Result<Guid>.Failure("A user with this email already exists.");

        return await ExecuteRegistrationAsync(request);
    }

    private async Task<Result<Guid>> ExecuteRegistrationAsync(TenantRegistrationRequest request)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var tenant = CreateTenant(request);
            var store = CreateStore(tenant.Id, request);
            var user = CreateUser(tenant.Id, request);
            var ownerRole = await CreateOwnerRoleAsync(tenant.Id);
            LinkUserToTenant(user.Id, tenant.Id, ownerRole.Id);
            CreateLegalInfo(tenant.Id, request);
            await CreateStarterSubscriptionAsync(tenant.Id);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            return Result<Guid>.Success(tenant.Id, "Registration submitted successfully. Awaiting approval.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();

            if (ex.InnerException?.Message.Contains("duplicate key") == true)
                return Result<Guid>.Failure("A record with the same details already exists.");

            throw;
        }
    }

    private TenantAccount CreateTenant(TenantRegistrationRequest request)
    {
        var slug = _slugGenerator.Generate(request.RestaurantName);
        var tenant = new TenantAccount
        {
            Id = Guid.NewGuid(),
            Name = request.RestaurantName,
            Slug = slug,
            IsActive = false,
            RegistrationStatus = RegistrationStatus.Pending
        };
        _tenantRepo.Add(tenant);
        return tenant;
    }

    private Store CreateStore(Guid tenantId, TenantRegistrationRequest request)
    {
        var store = new Store
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.RestaurantName,
            Slug = _slugGenerator.Generate(request.RestaurantName),
            Address = request.Address,
            MinPreparingTime = request.MinPreparingTime,
            MaxPreparingTime = request.MaxPreparingTime,
            IsOnline = false,
            StoreCuisines = new List<StoreCuisine>(),
            DeliveryZones = new List<StoreDeliveryZone>()
        };

        foreach (var c in request.CuisineIds)
            store.StoreCuisines.Add(new StoreCuisine { CuisineId = c });
        foreach (var z in request.ZoneIds)
            store.DeliveryZones.Add(new StoreDeliveryZone { ZoneId = z });

        _storeRepo.Add(store);
        return store;
    }

    private User CreateUser(Guid tenantId, TenantRegistrationRequest request)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = $"{request.FirstName} {request.LastName}",
            Phone = request.Phone,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            IsActive = true
        };
        _userRepo.Add(user);
        return user;
    }

    private async Task<Role> CreateOwnerRoleAsync(Guid tenantId)
    {
        var ownerRole = new Role
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "Owner",
            Description = "Tenant owner – full access",
            IsSystem = false
        };
        _roleRepo.Add(ownerRole);

        var allPermissions = await _roleRepo.GetAllPermissionsAsync();
        foreach (var perm in allPermissions)
            _roleRepo.AddRolePermission(new RolePermission { RoleId = ownerRole.Id, PermissionId = perm.Id });

        return ownerRole;
    }

    private void LinkUserToTenant(Guid userId, Guid tenantId, Guid roleId)
    {
        // Only UserRole is needed – User.TenantId already links the user to the tenant.
        _roleRepo.AddUserRole(new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            TenantId = tenantId
        });

        // ❌ Removed: _roleRepo.AddTenantUser(...) – TenantUser is obsolete.
    }

    private void CreateLegalInfo(Guid tenantId, TenantRegistrationRequest request)
    {
        _legalRepo.Add(new TenantLegalInfo
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CnicNumber = request.CnicNumber,
            NtnNumber = request.NtnNumber,
            HasFoodLicense = request.HasFoodLicense,
            CnicFrontImageUrl = request.CnicFrontImageUrl,
            CnicBackImageUrl = request.CnicBackImageUrl
        });
    }

    private async Task CreateStarterSubscriptionAsync(Guid tenantId)
    {
        var starterPlan = await _subscriptionRepo.GetPlanByNameAsync("Starter")
            ?? throw new InvalidOperationException("Starter plan not found.");

        _subscriptionRepo.Add(new Subscription
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PlanId = starterPlan.Id,
            Status = SubscriptionStatus.Trialing,
            StartDate = DateTime.UtcNow,
            TrialEndsAt = DateTime.UtcNow.AddDays(14),
            NextBillingDate = DateTime.UtcNow.AddDays(14),
            PriceSnapshot = 0
        });
    }
}