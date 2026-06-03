using CoreKit.Catalog.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Catalog.Persistence;

internal sealed class AddonGroupRepository : IAddonGroupRepository
{
    private readonly CatalogDbContext _db;
    public AddonGroupRepository(CatalogDbContext db) => _db = db;
    public Task<bool> ExistsForTenantAsync(Guid id, Guid tenantId, CancellationToken ct = default)
        => _db.AddonGroups.AnyAsync(g => g.Id == id && g.TenantId == tenantId, ct);
}