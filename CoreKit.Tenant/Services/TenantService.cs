// CoreKit.Tenant/Services/TenantService.cs
using CoreKit.IAM.Interfaces;
using CoreKit.SharedKernel.Helpers;
using CoreKit.Tenant.Entities;
using CoreKit.Tenant.Enums;
using CoreKit.Tenant.Interfaces;
using CoreKit.Tenant.Models;
using CoreKit.Tenant.Persistence;
using Microsoft.EntityFrameworkCore;
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

    public async Task<TenantRegistrationResponse> RegisterAsync(
        TenantRegistrationRequest request)
    {
        if (await _tenantRepo.ExistsByNameAsync(request.Name))
            throw new InvalidOperationException("Tenant already exists.");

        var slug = await GenerateUniqueSlugAsync(request.Name);

        var status = _options.AutoApproveTenants
            ? TenantStatus.Active
            : TenantStatus.Pending;

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
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException(
                "This record was modified by another user. " +
                "Please refresh and try again.");
        }

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

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException(
                "This record was modified by another user. " +
                "Please refresh and try again.");
        }
    }

    public async Task RejectAsync(Guid tenantId)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId)
            ?? throw new KeyNotFoundException("Tenant not found.");

        tenant.Status = TenantStatus.Rejected;
        _tenantRepo.Update(tenant);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException(
                "This record was modified by another user. " +
                "Please refresh and try again.");
        }
    }

    public async Task UpdateAsync(Guid tenantId, string? name, string? metadataJson)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId)
            ?? throw new KeyNotFoundException("Tenant not found.");

        if (name != null)
        {
            var newSlug = await GenerateUniqueSlugAsync(name, excludeTenantId: tenantId);
            tenant.Name = name;
            tenant.Slug = newSlug;
        }

        if (metadataJson != null)
            tenant.MetadataJson = metadataJson;

        _tenantRepo.Update(tenant);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException(
                "This record was modified by another user. " +
                "Please refresh and try again.");
        }
    }

    public async Task DeleteAsync(Guid tenantId)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId)
            ?? throw new KeyNotFoundException("Tenant not found.");

        tenant.IsDeleted = true;
        tenant.Status = TenantStatus.Archived;
        _tenantRepo.Update(tenant);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException(
                "This record was modified by another user. " +
                "Please refresh and try again.");
        }
    }

    // ---------------------------------------------------------------
    // Private helpers
    // ---------------------------------------------------------------

    private async Task<string> GenerateUniqueSlugAsync(
        string name,
        Guid? excludeTenantId = null)
    {
        var baseSlug = SlugHelper.Generate(name);
        var slug = baseSlug;
        var counter = 1;

        while (true)
        {
            var exists = await _db.Tenants.AnyAsync(t =>
                t.Slug == slug &&
                (excludeTenantId == null || t.Id != excludeTenantId.Value));

            if (!exists) return slug;

            slug = $"{baseSlug}-{counter++}";
        }
    }
}