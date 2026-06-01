using CoreKit.Tenant.Entities;
using CoreKit.Tenant.Interfaces;
using CoreKit.Tenant.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Tenant.Repositories;

public sealed class TenantLegalInfoRepository : ITenantLegalInfoRepository
{
    private readonly TenantDbContext _db;

    public TenantLegalInfoRepository(TenantDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<TenantLegalInfo?> GetByTenantAsync(
        Guid tenantId,
        CancellationToken ct = default)
    {
        return await _db.TenantLegalInfos
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.TenantId == tenantId,
                ct);
    }

    public async Task AddOrUpdateAsync(
        TenantLegalInfo info,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(info);

        try
        {
            var existing = await _db.TenantLegalInfos
                .SingleOrDefaultAsync(
                    x => x.TenantId == info.TenantId,
                    ct);

            if (existing is not null)
            {
                existing.BusinessLicenseNumber =
                    info.BusinessLicenseNumber;

                existing.TaxId =
                    info.TaxId;

                existing.AdditionalJson =
                    info.AdditionalJson;
            }
            else
            {
                await _db.TenantLegalInfos
                    .AddAsync(info, ct);
            }

            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException(
                "The tenant legal information was modified by another user. Refresh the data and try again.",
                ex);
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException(
                "Failed to save tenant legal information.",
                ex);
        }
    }
}