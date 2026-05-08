using Microsoft.EntityFrameworkCore;
using SaaSPlatform.Core.Catalog.Entities;
using SaaSPlatform.Core.Catalog.Interfaces;

namespace SaaSPlatform.Infrastructure.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly SaaSPlatformDbContext _db;
    public ProductRepository(SaaSPlatformDbContext db) => _db = db;

    public async Task<Product?> GetByIdAsync(Guid productId, CancellationToken ct)
        => await _db.Products.FirstOrDefaultAsync(p => p.Id == productId, ct);

    public async Task<List<Product>> GetByStoreAsync(Guid storeId, CancellationToken ct)
        => await _db.Products.Where(p => p.StoreId == storeId).ToListAsync(ct);

    public void Add(Product product) => _db.Products.Add(product);
    public void Update(Product product) => _db.Products.Update(product);
    public void Delete(Product product) => _db.Products.Remove(product);
}