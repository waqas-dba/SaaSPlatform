// CoreKit.Catalog | Services/VariantAttributeTemplateService.cs
using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.Catalog.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Catalog.Services;

public class VariantAttributeTemplateService : IVariantAttributeTemplateService
{
    private readonly CatalogDbContext _db;
    public VariantAttributeTemplateService(CatalogDbContext db) => _db = db;

    // Original platform endpoint (used by Admin controller)
    public async Task<List<VariantAttributeTemplateDto>> GetByStoreTypeAsync(string storeTypeCode)
    {
        return await _db.VariantAttributeTemplates
            .Where(t => t.StoreTypeCode == storeTypeCode && t.TenantId == null)
            .OrderBy(t => t.Name)
            .Select(t => new VariantAttributeTemplateDto
            {
                Id = t.Id,
                Name = t.Name,
                StoreTypeCode = t.StoreTypeCode,
                OptionsJson = t.OptionsJson
            })
            .ToListAsync();
    }

    // New platform methods (optional, explicit naming)
    public async Task<List<VariantAttributeTemplateDto>> GetPlatformTemplatesAsync(string storeTypeCode)
        => await GetByStoreTypeAsync(storeTypeCode);

    public async Task<VariantAttributeTemplateDto> CreatePlatformTemplateAsync(CreateVariantAttributeTemplateRequest request)
    {
        var template = new VariantAttributeTemplate
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            StoreTypeCode = request.StoreTypeCode.Trim(),
            OptionsJson = request.OptionsJson?.Trim(),
            TenantId = null
        };
        _db.VariantAttributeTemplates.Add(template);
        await _db.SaveChangesAsync();
        return new VariantAttributeTemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            StoreTypeCode = template.StoreTypeCode,
            OptionsJson = template.OptionsJson
        };
    }

    public async Task UpdatePlatformTemplateAsync(Guid id, UpdateVariantAttributeTemplateRequest request)
    {
        var template = await _db.VariantAttributeTemplates.FindAsync(id)
            ?? throw new KeyNotFoundException("Variant attribute template not found.");
        if (request.Name != null) template.Name = request.Name.Trim();
        if (request.OptionsJson != null)
            template.OptionsJson = string.IsNullOrWhiteSpace(request.OptionsJson) ? null : request.OptionsJson.Trim();
        await _db.SaveChangesAsync();
    }

    public async Task DeletePlatformTemplateAsync(Guid id)
    {
        var template = await _db.VariantAttributeTemplates.FindAsync(id)
            ?? throw new KeyNotFoundException("Variant attribute template not found.");
        _db.VariantAttributeTemplates.Remove(template);
        await _db.SaveChangesAsync();
    }

    // Tenant‑scoped methods
    public async Task<List<VariantAttributeTemplateDto>> GetTenantTemplatesAsync(Guid tenantId, string storeTypeCode)
    {
        return await _db.VariantAttributeTemplates
            .Where(t => t.StoreTypeCode == storeTypeCode && t.TenantId == tenantId)
            .OrderBy(t => t.Name)
            .Select(t => new VariantAttributeTemplateDto
            {
                Id = t.Id,
                Name = t.Name,
                StoreTypeCode = t.StoreTypeCode,
                OptionsJson = t.OptionsJson
            })
            .ToListAsync();
    }

    public async Task<VariantAttributeTemplateDto> CreateTenantTemplateAsync(Guid tenantId, CreateVariantAttributeTemplateRequest request)
    {
        var exists = await _db.VariantAttributeTemplates.AnyAsync(
            t => t.StoreTypeCode == request.StoreTypeCode && t.TenantId == tenantId && t.Name == request.Name.Trim());
        if (exists)
            throw new InvalidOperationException($"A variant attribute template named '{request.Name}' already exists for this store type.");

        var template = new VariantAttributeTemplate
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            StoreTypeCode = request.StoreTypeCode.Trim(),
            OptionsJson = request.OptionsJson?.Trim(),
            TenantId = tenantId
        };
        _db.VariantAttributeTemplates.Add(template);
        await _db.SaveChangesAsync();
        return new VariantAttributeTemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            StoreTypeCode = template.StoreTypeCode,
            OptionsJson = template.OptionsJson
        };
    }

    public async Task UpdateTenantTemplateAsync(Guid id, Guid tenantId, UpdateVariantAttributeTemplateRequest request)
    {
        var template = await _db.VariantAttributeTemplates.FirstOrDefaultAsync(
            t => t.Id == id && t.TenantId == tenantId)
            ?? throw new KeyNotFoundException("Tenant variant attribute template not found.");
        if (request.Name != null) template.Name = request.Name.Trim();
        if (request.OptionsJson != null)
            template.OptionsJson = string.IsNullOrWhiteSpace(request.OptionsJson) ? null : request.OptionsJson.Trim();
        await _db.SaveChangesAsync();
    }

    public async Task DeleteTenantTemplateAsync(Guid id, Guid tenantId)
    {
        var template = await _db.VariantAttributeTemplates.FirstOrDefaultAsync(
            t => t.Id == id && t.TenantId == tenantId)
            ?? throw new KeyNotFoundException("Tenant variant attribute template not found.");
        _db.VariantAttributeTemplates.Remove(template);
        await _db.SaveChangesAsync();
    }
}