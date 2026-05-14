using AuthCoreKit.IAM.Entities;
using AuthCoreKit.IAM.Interfaces;
using AuthCoreKit.IAM.Models;
using Microsoft.Extensions.DependencyInjection;
using SaaSPlatform.Application.Models;
using SaaSPlatform.Core.Billing.Entities;
using SaaSPlatform.Core.Billing.Enums;
using SaaSPlatform.Core.Billing.Interfaces;
using SaaSPlatform.SharedKernel.Interfaces;
using SaaSPlatform.SharedKernel.Results;
using TenantKit.Interfaces;
using TenantKit.Models;

namespace SaaSPlatform.Application.Services;

public class TenantRegistrationAppService
{
    private readonly ITenantService _tenantService;
    private readonly IStoreService _storeService;
    private readonly IUserRepository _userRepo;
    private readonly IRoleRepository _roleRepo;
    private readonly ISubscriptionRepository _subscriptionRepo;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IamOptions _iamOptions;
    private readonly IServiceProvider _serviceProvider;

    // ⚠️ No repository for cuisines or zones – they belong to other modules.

    public TenantRegistrationAppService(
        ITenantService tenantService,
        IStoreService storeService,
        IUserRepository userRepo,
        IRoleRepository roleRepo,
        ISubscriptionRepository subscriptionRepo,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IamOptions iamOptions,
        IServiceProvider serviceProvider)
    {
        _tenantService = tenantService;
        _storeService = storeService;
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _subscriptionRepo = subscriptionRepo;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _iamOptions = iamOptions;
        _serviceProvider = serviceProvider;
    }

    public async Task<Result<Guid>> RegisterAsync(HostTenantRegistrationRequest request)
    {
        if (await _userRepo.ExistsByPhoneAsync(request.Phone, tenantId: null))
            return Result<Guid>.Failure("A user with this phone number already exists.");

        if (!string.IsNullOrWhiteSpace(request.Email) &&
            await _userRepo.ExistsByEmailAsync(request.Email, tenantId: null))
            return Result<Guid>.Failure("A user with this email already exists.");

        return await ExecuteRegistrationAsync(request);
    }

    private async Task<Result<Guid>> ExecuteRegistrationAsync(HostTenantRegistrationRequest request)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // 1. Create tenant via TenantKit (generic, no domain data)
            var tenantResponse = await _tenantService.RegisterAsync(new TenantRegistrationRequest
            {
                Name = request.RestaurantName,
                MetadataJson = request.MetadataJson
            });

            // 2. Create the first store (minimal info – type "Restaurant" but no cuisines/zones)
            var store = await _storeService.CreateAsync(
                tenantResponse.TenantId,
                request.RestaurantName,
                type: "Restaurant",
                metadataJson: null);

            // 3. Create IAM user (owner)
            var user = CreateUser(tenantResponse.TenantId, request);
            _userRepo.Add(user);

            // 4. Create owner role and assign all permissions
            var ownerRole = await CreateOwnerRoleAsync(tenantResponse.TenantId);

            // 5. Link user to role within tenant
            _roleRepo.AddUserRole(new UserRole
            {
                UserId = user.Id,
                RoleId = ownerRole.Id,
                TenantId = tenantResponse.TenantId
            });

            // 6. Starter subscription (billing)
            await CreateStarterSubscriptionAsync(tenantResponse.TenantId);

            // 7. CNIC encryption (if required)
            if (_iamOptions.EnableUserIdentities && _iamOptions.RequireCnic)
            {
                if (string.IsNullOrWhiteSpace(request.CnicNumber))
                    throw new InvalidOperationException("CNIC number is required.");

                var cnic = request.CnicNumber.Replace("-", "").Trim();
                if (cnic.Length != 13 || !cnic.All(char.IsDigit))
                    throw new InvalidOperationException("Invalid CNIC number format.");

                var identityService = _serviceProvider.GetRequiredService<IUserIdentityService>();
                await identityService.SetCnicAsync(user.Id, cnic);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            return Result<Guid>.Success(tenantResponse.TenantId,
                "Registration submitted successfully. Awaiting approval.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            if (ex is InvalidOperationException) return Result<Guid>.Failure(ex.Message);
            throw;
        }
    }

    private User CreateUser(Guid tenantId, HostTenantRegistrationRequest request) => new User
    {
        Id = Guid.NewGuid(),
        TenantId = tenantId,
        Name = $"{request.FirstName} {request.LastName}",
        Phone = request.Phone,
        Email = request.Email,
        PasswordHash = _passwordHasher.Hash(request.Password),
        IsActive = true
    };

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