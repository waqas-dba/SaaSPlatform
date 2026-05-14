using CoreKit.Tenant.Entities;
using CoreKit.Tenant.Enums;
using CoreKit.Tenant.Interfaces;
using CoreKit.Tenant.Models;

namespace CoreKit.Tenant.Services;

public class TenantService : ITenantService
{
    private readonly ITenantRepository _tenantRepo;
    private readonly TenantKitOptions _options;

    public TenantService(ITenantRepository tenantRepo, TenantKitOptions options)
    {
        _tenantRepo = tenantRepo;
        _options = options;
    }

    public async Task<TenantRegistrationResponse> RegisterAsync(TenantRegistrationRequest request)
    {
        if (await _tenantRepo.ExistsByNameAsync(request.Name))
            throw new InvalidOperationException("A tenant with this name already exists.");

        var initialStatus = _options.AutoApproveTenants ? TenantStatus.Active : TenantStatus.Pending;

        var tenant = new TenantEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Slug = GenerateSlug(request.Name),
            Status = initialStatus,
            MetadataJson = request.MetadataJson,
            CreatedAt = DateTime.UtcNow
        };
        _tenantRepo.Add(tenant);

        return new TenantRegistrationResponse
        {
            TenantId = tenant.Id,
            Message = initialStatus == TenantStatus.Active
                ? "Tenant registered and active."
                : "Tenant registered. Awaiting approval."
        };
    }

    public async Task<TenantEntity?> GetByIdAsync(Guid tenantId) => await _tenantRepo.GetByIdAsync(tenantId);
    public async Task<List<TenantEntity>> GetAllAsync() => await _tenantRepo.GetAllAsync();

    public async Task UpdateAsync(Guid tenantId, string? name, string? metadataJson)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId) ?? throw new KeyNotFoundException("Tenant not found.");
        if (name != null) tenant.Name = name;
        if (metadataJson != null) tenant.MetadataJson = metadataJson;
        _tenantRepo.Update(tenant);
    }

    public async Task DeleteAsync(Guid tenantId)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId) ?? throw new KeyNotFoundException("Tenant not found.");
        tenant.IsDeleted = true;
        tenant.DeletedAtUtc = DateTime.UtcNow;
        tenant.Status = TenantStatus.Archived;
        _tenantRepo.Update(tenant);
    }

    public async Task ApproveAsync(Guid tenantId)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId) ?? throw new KeyNotFoundException("Tenant not found.");
        tenant.Status = TenantStatus.Active;
        _tenantRepo.Update(tenant);
    }

    public async Task RejectAsync(Guid tenantId)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId) ?? throw new KeyNotFoundException("Tenant not found.");
        tenant.Status = TenantStatus.Rejected;
        _tenantRepo.Update(tenant);
    }

    private static string GenerateSlug(string name)
        => name.ToLowerInvariant().Replace(" ", "-") + "-" + Guid.NewGuid().ToString("N")[..6];
}