using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Catalog.Persistence;

public sealed class AddonGroupRepository : IAddonGroupRepository
{
    private readonly CatalogDbContext _db;

    public AddonGroupRepository(CatalogDbContext db) => _db = db;

    public Task<AddonGroup?> GetByIdAsync(
        Guid tenantId, Guid id, bool track, CancellationToken ct = default)
    {
        IQueryable<AddonGroup> query = _db.AddonGroups
            .Where(g => g.TenantId == tenantId && g.Id == id)
            .Include(g => g.Addons);

        if (!track) query = query.AsNoTracking();
        return query.FirstOrDefaultAsync(ct);
    }

    public Task<List<AddonGroup>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => _db.AddonGroups
            .AsNoTracking()
            .Where(g => g.TenantId == tenantId)
            .Include(g => g.Addons)
            .OrderBy(g => g.SortOrder)
            .ThenBy(g => g.Name)
            .ToListAsync(ct);

    public Task<int> CountExistingAsync(
        Guid tenantId, IReadOnlyCollection<Guid> ids, CancellationToken ct = default)
        => _db.AddonGroups.CountAsync(g => g.TenantId == tenantId && ids.Contains(g.Id), ct);

    public Task<bool> NameExistsAsync(
        Guid tenantId, string name, Guid? excludeId, CancellationToken ct = default)
        => _db.AddonGroups.AnyAsync(
            g => g.TenantId == tenantId && g.Name == name && (excludeId == null || g.Id != excludeId), ct);

    public Task<bool> IsInUseAsync(Guid groupId, CancellationToken ct = default)
        => _db.ProductAddonGroups.AnyAsync(
            l => l.AddonGroupId == groupId && !l.Product.IsDeleted, ct);

    public void Add(AddonGroup group) => _db.AddonGroups.Add(group);
    public void Remove(AddonGroup group) => _db.AddonGroups.Remove(group);
    public void AddAddon(Addon addon) => _db.Addons.Add(addon);
    public void RemoveAddon(Addon addon) => _db.Addons.Remove(addon);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}