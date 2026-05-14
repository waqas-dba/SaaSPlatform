using Microsoft.EntityFrameworkCore;
using TenantKit.Entities;

namespace TenantKit.Interfaces;

public interface ITenantKitDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<Store> Stores { get; }
    DbSet<TenantLegalInfo>? TenantLegalInfos { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}