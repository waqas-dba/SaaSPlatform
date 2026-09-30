using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CoreKit.Catalog.Persistence;

public sealed class StoreProductRepository : IStoreProductRepository
{
    private readonly CatalogDbContext _db;
    private IDbContextTransaction? _currentTransaction;

    public StoreProductRepository(CatalogDbContext db) => _db = db;

    public Task<StoreProduct?> GetAsync(
        Guid tenantId, Guid storeId, Guid productId, CancellationToken ct = default)
        => _db.StoreProducts.FirstOrDefaultAsync(
            sp => sp.TenantId == tenantId && sp.StoreId == storeId && sp.ProductId == productId, ct);

    public Task<List<StoreProduct>> GetByStoreAsync(
        Guid tenantId, Guid storeId, CancellationToken ct = default)
        => _db.StoreProducts
            .Where(sp => sp.TenantId == tenantId && sp.StoreId == storeId)
            .ToListAsync(ct);

    public async Task<HashSet<Guid>> GetProductIdsAsync(
        Guid tenantId, Guid storeId, CancellationToken ct = default)
    {
        var ids = await _db.StoreProducts
            .AsNoTracking()
            .Where(sp => sp.TenantId == tenantId && sp.StoreId == storeId)
            .Select(sp => sp.ProductId)
            .ToListAsync(ct);

        return new HashSet<Guid>(ids);
    }

    public void Add(StoreProduct storeProduct) => _db.StoreProducts.Add(storeProduct);
    public void Remove(StoreProduct storeProduct) => _db.StoreProducts.Remove(storeProduct);
    public void RemoveRange(IEnumerable<StoreProduct> storeProducts) => _db.StoreProducts.RemoveRange(storeProducts);

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        _currentTransaction = await _db.Database.BeginTransactionAsync(ct);
        return _currentTransaction;
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        if (_currentTransaction is null)
            throw new InvalidOperationException("No active transaction to commit.");

        await _currentTransaction.CommitAsync(ct);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (_currentTransaction is null) return;

        await _currentTransaction.RollbackAsync(ct);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    public Task ExecuteAdvisoryLockAsync(long lockKey, CancellationToken ct = default)
        => _db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock({0})", new object[] { lockKey }, ct);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}