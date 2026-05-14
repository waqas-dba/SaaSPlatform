using TenantKit.Entities;
using TenantKit.Interfaces;

namespace TenantKit.Services;

public class TenantLegalInfoService : ITenantLegalInfoService
{
    private readonly ITenantLegalInfoRepository _repo;
    public TenantLegalInfoService(ITenantLegalInfoRepository repo) => _repo = repo;

    public async Task AddOrUpdateAsync(Guid tenantId, string? businessLicenseNumber, string? taxId, string? additionalJson)
    {
        var info = new TenantLegalInfo
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            BusinessLicenseNumber = businessLicenseNumber,
            TaxId = taxId,
            AdditionalJson = additionalJson
        };
        await _repo.AddOrUpdateAsync(info);
    }

    public async Task<TenantLegalInfo?> GetAsync(Guid tenantId) => await _repo.GetByTenantAsync(tenantId);
}