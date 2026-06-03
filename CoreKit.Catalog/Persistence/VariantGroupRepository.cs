using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Catalog.Persistence;

internal sealed class VariantGroupRepository : IVariantGroupRepository
{
    private readonly CatalogDbContext _db;
    public VariantGroupRepository(CatalogDbContext db) => _db = db;
    public Task<VariantGroup?> GetByIdWithOptionsAsync(Guid id, Guid tenantId, CancellationToken ct = default)
        => _db.VariantGroups
            .Include(g => g.Options).ThenInclude(o => o.Template)
            .FirstOrDefaultAsync(g => g.Id == id && g.TenantId == tenantId, ct);
}