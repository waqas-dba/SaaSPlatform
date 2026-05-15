using CoreKit.IAM.Interfaces;
using CoreKit.Tenant.Entities;
using CoreKit.Tenant.Enums;
using CoreKit.Tenant.Interfaces;
using CoreKit.Tenant.Models;
using CoreKit.Tenant.Persistence;
using Microsoft.Extensions.Options;

namespace CoreKit.Tenant.Services;

/// <summary>
/// Tenant management service.
/// </summary>
public class TenantService : ITenantService
{
    private readonly ITenantRepository _tenantRepo;
    private readonly TenantKitOptions _options;
    private readonly TenantDbContext _db;
    private readonly ICurrentUserService? _currentUser;   // Null if no authenticated user

    public TenantService(
        ITenantRepository tenantRepo,
        IOptions<TenantKitOptions> options,
        TenantDbContext db,
        ICurrentUserService? currentUser = null)
    {
        _tenantRepo = tenantRepo;
        _options = options.Value;
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<TenantRegistrationResponse> RegisterAsync(TenantRegistrationRequest request)
    {
        if (await _tenantRepo.ExistsByNameAsync(request.Name))
            throw new InvalidOperationException("Tenant already exists.");

        var status = _options.AutoApproveTenants ? TenantStatus.Active : TenantStatus.Pending;

        var tenant = new TenantEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Slug = request.Name.ToLower().Replace(" ", "-"),
            Status = status,
            OwnerUserId = _currentUser?.UserId   // Set if authenticated
        };

        _tenantRepo.Add(tenant);
        await _db.SaveChangesAsync();   // Critical: persist changes

        return new TenantRegistrationResponse
        {
            TenantId = tenant.Id,
            Message = "Tenant created successfully"
        };
    }

    public Task<TenantEntity?> GetByIdAsync(Guid tenantId)
        => _tenantRepo.GetByIdAsync(tenantId);

    public Task<List<TenantEntity>> GetAllAsync()
        => _tenantRepo.GetAllAsync();

    public async Task ApproveAsync(Guid tenantId)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId)
                     ?? throw new KeyNotFoundException("Tenant not found");
        tenant.Status = TenantStatus.Active;
        _tenantRepo.Update(tenant);
        await _db.SaveChangesAsync();
    }

    public async Task RejectAsync(Guid tenantId)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId)
                     ?? throw new KeyNotFoundException("Tenant not found");
        tenant.Status = TenantStatus.Rejected;
        _tenantRepo.Update(tenant);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Guid tenantId, string? name, string? metadataJson)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId)
                     ?? throw new KeyNotFoundException("Tenant not found");
        if (name != null) tenant.Name = name;
        if (metadataJson != null) tenant.MetadataJson = metadataJson;
        _tenantRepo.Update(tenant);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Soft‑deletes a tenant by marking it as Archived and setting IsDeleted.
    /// </summary>
    public async Task DeleteAsync(Guid tenantId)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId)
                     ?? throw new KeyNotFoundException("Tenant not found");

        tenant.IsDeleted = true;
        tenant.Status = TenantStatus.Archived;
        // AuditableDbContext will automatically set DeletedAtUtc/DeletedBy
        _tenantRepo.Update(tenant);
        await _db.SaveChangesAsync();
    }
}