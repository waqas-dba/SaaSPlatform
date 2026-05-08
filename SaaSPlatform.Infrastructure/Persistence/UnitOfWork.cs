using Microsoft.EntityFrameworkCore.Storage;
using SaaSPlatform.Infrastructure.Persistence;
using SaaSPlatform.SharedKernel.Interfaces;

namespace SaaSPlatform.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly SaaSPlatformDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(SaaSPlatformDbContext context) => _context = context;

    public async Task BeginTransactionAsync(CancellationToken ct = default)
        => _transaction = await _context.Database.BeginTransactionAsync(ct);

    public async Task CommitAsync(CancellationToken ct = default)
    { if (_transaction != null) await _transaction.CommitAsync(ct); }

    public async Task RollbackAsync(CancellationToken ct = default)
    { if (_transaction != null) await _transaction.RollbackAsync(ct); }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);

    public void Dispose() => _transaction?.Dispose();
}