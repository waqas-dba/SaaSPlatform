using CoreKit.IAM.Interfaces;
using CoreKit.Tenant.Entities;
using CoreKit.Tenant.Enums;
using CoreKit.Tenant.Interfaces;
using CoreKit.Tenant.Models;
using CoreKit.Tenant.Persistence;
using Microsoft.Extensions.Options;

namespace CoreKit.Tenant.Services;

public class TenantService : ITenantService
{
    private readonly ITenantRepository _tenantRepo;
    private readonly TenantKitOptions _options;
    private readonly TenantDbContext _db;
    private readonly ICurrentUserService? _currentUser;

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

        var status = _options.AutoApproveTenants
            ? TenantStatus.Active
            : TenantStatus.Pending;

        // Use the same safe slug generator used in StoreService — strips special chars
        var slug = GenerateSlug(request.Name);

        var tenant = new TenantEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Slug = slug,
            Status = status,
            MetadataJson = request.MetadataJson,
            OwnerUserId = _currentUser?.UserId
        };

        _tenantRepo.Add(tenant);
        await _db.SaveChangesAsync();

        return new TenantRegistrationResponse
        {
            TenantId = tenant.Id,
            Message = _options.AutoApproveTenants
                ? "Tenant registered and activated."
                : "Tenant registration submitted. Awaiting approval."
        };
    }

    public Task<TenantEntity?> GetByIdAsync(Guid tenantId)
        => _tenantRepo.GetByIdAsync(tenantId);

    public Task<List<TenantEntity>> GetAllAsync()
        => _tenantRepo.GetAllAsync();

    public async Task ApproveAsync(Guid tenantId)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId)
            ?? throw new KeyNotFoundException("Tenant not found.");

        if (tenant.Status == TenantStatus.Active)
            throw new InvalidOperationException("Tenant is already active.");

        tenant.Status = TenantStatus.Active;
        _tenantRepo.Update(tenant);
        await _db.SaveChangesAsync();
    }

    public async Task RejectAsync(Guid tenantId)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId)
            ?? throw new KeyNotFoundException("Tenant not found.");

        tenant.Status = TenantStatus.Rejected;
        _tenantRepo.Update(tenant);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Guid tenantId, string? name, string? metadataJson)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId)
            ?? throw new KeyNotFoundException("Tenant not found.");

        if (name != null)
        {
            tenant.Name = name;
            tenant.Slug = GenerateSlug(name);
        }

        if (metadataJson != null)
            tenant.MetadataJson = metadataJson;

        _tenantRepo.Update(tenant);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid tenantId)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId)
            ?? throw new KeyNotFoundException("Tenant not found.");

        tenant.IsDeleted = true;
        tenant.Status = TenantStatus.Archived;
        _tenantRepo.Update(tenant);
        await _db.SaveChangesAsync();
    }

    // Strips special characters so slugs are safe for URLs and routing.
    // "A&B Corp" → "ab-corp", "Héllo Wörld" → "hllo-wrld" (safe fallback)
    private static string GenerateSlug(string name)
    {
        var slug = name.Trim().ToLowerInvariant();
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
        return slug.Trim('-');
    }
}