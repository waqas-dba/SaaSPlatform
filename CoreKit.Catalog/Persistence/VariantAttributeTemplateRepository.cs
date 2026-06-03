using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Catalog.Persistence;

internal sealed class VariantAttributeTemplateRepository : IVariantAttributeTemplateRepository
{
    private readonly CatalogDbContext _db;
    public VariantAttributeTemplateRepository(CatalogDbContext db) => _db = db;
    public Task<VariantAttributeTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.VariantAttributeTemplates.FirstOrDefaultAsync(t => t.Id == id, ct);
    public Task<VariantAttributeTemplate?> GetByNameAndStoreTypeAsync(string name, string storeTypeCode, CancellationToken ct = default)
        => _db.VariantAttributeTemplates.FirstOrDefaultAsync(t => t.Name == name && t.StoreTypeCode == storeTypeCode, ct);
}