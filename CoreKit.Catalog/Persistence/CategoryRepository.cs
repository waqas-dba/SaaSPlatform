using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Catalog.Persistence;

internal sealed class CategoryRepository : ICategoryRepository
{
    private readonly CatalogDbContext _db;
    public CategoryRepository(CatalogDbContext db) => _db = db;
    public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);
    public Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => _db.Categories.AnyAsync(c => c.Id == id, ct);
}